using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var serviceProvider = application.ApplicationServices;
            var pipelineLogger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Nop.Pipeline");
            pipelineLogger.LogDebug("Starting pipeline configuration");

            // Initialize nopCommerce Engine
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

            var containerManager = new ContainerManager(autofacContainer);
            var engine = new AspNetCoreNopEngine(containerManager);
            EngineContext.Replace(engine);
            pipelineLogger.LogDebug("EngineContext replaced");

            var nopConfig = new NopConfig();
            engine.Initialize(nopConfig);
            pipelineLogger.LogInformation("Engine initialized");

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
                        pipelineLogger.LogWarning(ex, "Startup task failed. Task={Task}", startUpTask.GetType().Name);
                    }
                }
            }

            // Redirect to installation page if database is not installed
            // This must be early in the pipeline, before any database-dependent services are resolved
            var installRedirectLogger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Nop.InstallRedirect");
            application.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
                if (!path.StartsWith("/install") && !path.StartsWith("/content") && !path.StartsWith("/scripts") && !path.StartsWith("/images"))
                {
                    if (!Nop.Core.Data.DataSettingsHelper.DatabaseIsInstalled())
                    {
                        installRedirectLogger.LogDebug("Redirecting to install. Path={Path}, TraceId={TraceId}", path, context.TraceIdentifier);
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
            application.UseEndpoints(endpoints =>
            {
                var routeLogger = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Nop.RouteRegistration");
                routeLogger.LogDebug("Configuring endpoints");
                try
                {
                    var routePublisher = EngineContext.Current.Resolve<IRoutePublisher>();
                    routeLogger.LogInformation("IRoutePublisher resolved. Type={Type}", routePublisher.GetType().Name);
                    routePublisher.RegisterRoutes(endpoints);
                    routeLogger.LogInformation("Routes registered via RoutePublisher");
                }
                catch (Exception ex)
                {
                    routeLogger.LogWarning(ex, "Could not resolve IRoutePublisher. Message={Message}", ex.Message);
                    try
                    {
                        var fallbackTypeFinder = EngineContext.Current.Resolve<ITypeFinder>();
                        var routePublisherManual = new RoutePublisher(fallbackTypeFinder);
                        routePublisherManual.RegisterRoutes(endpoints);
                        routeLogger.LogInformation("Fallback route registration succeeded");
                    }
                    catch (Exception ex2)
                    {
                        routeLogger.LogWarning(ex2, "Fallback route registration failed. Message={Message}", ex2.Message);
                        routeLogger.LogInformation("Registering fallback routes (HomePage, Install, default)");
                        endpoints.MapControllerRoute(
                            name: "HomePage",
                            pattern: "",
                            defaults: new { controller = "Home", action = "Index" });
                        endpoints.MapControllerRoute(
                            name: "Install",
                            pattern: "Install/{action=Index}/{id?}",
                            defaults: new { controller = "Install", action = "Index" });
                        endpoints.MapControllerRoute(
                            name: "default",
                            pattern: "{controller=Home}/{action=Index}/{id?}");
                        routeLogger.LogInformation("Fallback routes registered");
                    }
                }

                endpoints.MapControllers();
                routeLogger.LogDebug("Endpoint configuration complete");
            });
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
