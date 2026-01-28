using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Framework.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nop.Web.Framework.Infrastructure.Extensions
{
    /// <summary>
    /// Represents extensions of IApplicationBuilder
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configure the application HTTP request pipeline
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void ConfigureRequestPipeline(this IApplicationBuilder application)
        {
            Console.WriteLine("[LOG] ConfigureRequestPipeline: Starting pipeline configuration...");
            
            var serviceProvider = application.ApplicationServices;
            
            // Initialize nopCommerce Engine
            // Get the Autofac container from the service provider
            Console.WriteLine("[LOG] ConfigureRequestPipeline: Getting Autofac root container...");
            var autofacRoot = serviceProvider.GetAutofacRoot();
            IContainer autofacContainer;
            if (autofacRoot is IContainer container)
            {
                autofacContainer = container;
            }
            else
            {
                throw new InvalidOperationException("Autofac root scope is not an IContainer. Cannot initialize nopCommerce engine.");
            }

            Console.WriteLine("[LOG] ConfigureRequestPipeline: Creating ContainerManager...");
            var containerManager = new ContainerManager(autofacContainer);

            // Create a custom engine adapter that uses the ASP.NET Core Autofac container
            Console.WriteLine("[LOG] ConfigureRequestPipeline: Creating AspNetCoreNopEngine...");
            var engine = new AspNetCoreNopEngine(containerManager);
            EngineContext.Replace(engine);
            Console.WriteLine("[LOG] ConfigureRequestPipeline: EngineContext replaced.");

            // Load NopConfig (should match what was registered in ConfigureApplicationServices)
            var nopConfig = new NopConfig();

            // Initialize the engine (registers mapper configurations)
            Console.WriteLine("[LOG] ConfigureRequestPipeline: Initializing engine...");
            engine.Initialize(nopConfig);
            Console.WriteLine("[LOG] ConfigureRequestPipeline: Engine initialized.");

            // Run startup tasks if not ignored
            if (!nopConfig.IgnoreStartupTasks)
            {
                var typeFinder = new WebAppTypeFinder();
                var startUpTaskTypes = typeFinder.FindClassesOfType<IStartupTask>();
                var startUpTasks = startUpTaskTypes
                    .Select(type => (IStartupTask)Activator.CreateInstance(type))
                    .OrderBy(st => st.Order)
                    .ToList();

                foreach (var startUpTask in startUpTasks)
                {
                    try
                    {
                        startUpTask.Execute();
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue
                        Console.WriteLine($"Error executing startup task {startUpTask.GetType().Name}: {ex.Message}");
                    }
                }
            }

            // Redirect to installation page if database is not installed
            // This must be early in the pipeline, before any database-dependent services are resolved
            application.Use(async (context, next) =>
            {
                // Skip installation check for /install routes
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
                if (!path.StartsWith("/install") && !path.StartsWith("/content") && !path.StartsWith("/scripts") && !path.StartsWith("/images"))
                {
                    if (!Nop.Core.Data.DataSettingsHelper.DatabaseIsInstalled())
                    {
                        context.Response.Redirect("/install");
                        return;
                    }
                }
                await next();
            });

            // Get environment from service provider
            var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            
            // ASP.NET Core 8 middleware pipeline
            // Order is critical - must be in this sequence
            if (environment.IsDevelopment())
            {
                application.UseDeveloperExceptionPage();
            }
            else
            {
                application.UseExceptionHandler("/Home/Error");
                application.UseHsts();
            }

            // Only use HTTPS redirection in production
            if (!environment.IsDevelopment())
            {
                application.UseHttpsRedirection();
            }
            
            // Configure static files from wwwroot (default)
            application.UseStaticFiles();
            
            // Configure static files from Content folder (legacy nopCommerce structure)
            var contentPath = Path.Combine(environment.ContentRootPath, "Content");
            if (Directory.Exists(contentPath))
            {
                application.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(contentPath),
                    RequestPath = "/Content"
                });
            }
            
            // Configure static files from Scripts folder (legacy nopCommerce structure)
            var scriptsPath = Path.Combine(environment.ContentRootPath, "Scripts");
            if (Directory.Exists(scriptsPath))
            {
                application.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(scriptsPath),
                    RequestPath = "/Scripts"
                });
            }
            
            // Configure static files from Administration/Content folder (admin area)
            var adminContentPath = Path.Combine(environment.ContentRootPath, "Administration", "Content");
            if (Directory.Exists(adminContentPath))
            {
                application.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(adminContentPath),
                    RequestPath = "/Administration/Content"
                });
            }
            else
            {
                // Try alternative location - might be in Areas/Administration
                var areasAdminPath = Path.Combine(environment.ContentRootPath, "Areas", "Administration", "Content");
                if (Directory.Exists(areasAdminPath))
                {
                    application.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(areasAdminPath),
                        RequestPath = "/Administration/Content"
                    });
                }
            }

            // Routing must come before Session, Authentication, Authorization
            application.UseRouting();

            application.UseSession();
            application.UseAuthentication();
            application.UseAuthorization();

            // Register endpoints (routes) using UseEndpoints
            Console.WriteLine("[LOG] ConfigureRequestPipeline: Configuring endpoints...");
            application.UseEndpoints(endpoints =>
            {
                // Register nopCommerce routes using RoutePublisher
                // RoutePublisher is registered in Autofac, so we need to resolve it from EngineContext
                Console.WriteLine("[LOG] ConfigureRequestPipeline: Attempting to resolve IRoutePublisher...");
                try
                {
                    var routePublisher = EngineContext.Current.Resolve<IRoutePublisher>();
                    Console.WriteLine($"[LOG] ConfigureRequestPipeline: IRoutePublisher resolved: {routePublisher.GetType().Name}");
                    Console.WriteLine("[LOG] ConfigureRequestPipeline: Registering routes via RoutePublisher...");
                    routePublisher.RegisterRoutes(endpoints);
                    Console.WriteLine("[LOG] ConfigureRequestPipeline: Routes registered successfully.");
                }
                catch (Exception ex)
                {
                    // If route registration fails, try manual creation as fallback
                    Console.WriteLine($"[LOG] ERROR: Could not resolve IRoutePublisher from container: {ex.Message}");
                    Console.WriteLine($"[LOG] ERROR: Stack trace: {ex.StackTrace}");
                    try
                    {
                        Console.WriteLine("[LOG] ConfigureRequestPipeline: Attempting fallback route registration...");
                        var fallbackTypeFinder = EngineContext.Current.Resolve<ITypeFinder>();
                        var routePublisherManual = new RoutePublisher(fallbackTypeFinder);
                        routePublisherManual.RegisterRoutes(endpoints);
                        Console.WriteLine("[LOG] ConfigureRequestPipeline: Fallback route registration succeeded.");
                    }
                    catch (Exception ex2)
                    {
                        // If all else fails, register default route
                        Console.WriteLine($"[LOG] ERROR: Fallback route registration failed: {ex2.Message}");
                        Console.WriteLine($"[LOG] ERROR: Stack trace: {ex2.StackTrace}");
                        Console.WriteLine("[LOG] ConfigureRequestPipeline: Registering default route only...");
                        endpoints.MapControllerRoute(
                            name: "default",
                            pattern: "{controller=Home}/{action=Index}/{id?}");
                        Console.WriteLine("[LOG] ConfigureRequestPipeline: Default route registered.");
                    }
                }

                // Map all controllers (convention-based routing)
                Console.WriteLine("[LOG] ConfigureRequestPipeline: Mapping all controllers...");
                endpoints.MapControllers();
                Console.WriteLine("[LOG] ConfigureRequestPipeline: Endpoint configuration complete.");
            });
            Console.WriteLine("[LOG] ConfigureRequestPipeline: Pipeline configuration complete.");
        }
    }

    /// <summary>
    /// Custom NopEngine adapter that uses ASP.NET Core's Autofac container
    /// </summary>
    internal class AspNetCoreNopEngine : IEngine
    {
        private readonly ContainerManager _containerManager;

        public AspNetCoreNopEngine(ContainerManager containerManager)
        {
            _containerManager = containerManager;
        }

        public ContainerManager ContainerManager => _containerManager;

        public void Initialize(NopConfig config)
        {
            // Already initialized via ASP.NET Core DI container
            // Just register mapper configurations if needed
            var typeFinder = new WebAppTypeFinder();
            RegisterMapperConfiguration(config, typeFinder);
        }

        private void RegisterMapperConfiguration(NopConfig config, ITypeFinder mapperTypeFinder)
        {
            // Register mapper configurations provided by other assemblies
            var mcTypes = mapperTypeFinder.FindClassesOfType<IMapperConfiguration>();
            var mcInstances = mcTypes
                .Select(mcType => (IMapperConfiguration)Activator.CreateInstance(mcType))
                .OrderBy(mc => mc.Order)
                .ToList();

            // Get configurations
            var configurationActions = new List<Action<AutoMapper.IMapperConfigurationExpression>>();
            foreach (var mc in mcInstances)
            {
                configurationActions.Add(mc.GetConfiguration());
            }

            // Register
            AutoMapperConfiguration.Init(configurationActions);
        }

        public T Resolve<T>() where T : class
        {
            return ContainerManager.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return ContainerManager.Resolve(type);
        }

        public T[] ResolveAll<T>()
        {
            return ContainerManager.ResolveAll<T>();
        }
    }
}
