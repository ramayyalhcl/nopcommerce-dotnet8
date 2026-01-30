using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using System;
using System.Linq;

namespace Nop.Web.Framework.Infrastructure.Extensions
{
    /// <summary>
    /// Represents extensions of IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure application services
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="builder">A builder for web applications and services</param>
        public static void ConfigureApplicationServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            Console.WriteLine("[LOG] ConfigureApplicationServices: Starting service configuration...");
            
            // Configure basic ASP.NET Core services
            Console.WriteLine("[LOG] ConfigureApplicationServices: Adding HttpContextAccessor...");
            services.AddHttpContextAccessor();
            
            // AddControllersWithViews automatically registers IUrlHelperFactory, but IActionContextAccessor needs explicit registration
            Console.WriteLine("[LOG] ConfigureApplicationServices: Adding ControllersWithViews...");
            var mvcBuilder = services.AddControllersWithViews();
            mvcBuilder.AddRazorRuntimeCompilation();
            
            // Explicitly register IActionContextAccessor (required by WebHelper)
            Console.WriteLine("[LOG] ConfigureApplicationServices: Registering IActionContextAccessor...");
            services.AddSingleton<Microsoft.AspNetCore.Mvc.Infrastructure.IActionContextAccessor, Microsoft.AspNetCore.Mvc.Infrastructure.ActionContextAccessor>();
            
            services.AddRazorPages();

            // Configure session with options
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Configure authentication (Cookie-based for nopCommerce)
            // NopCookie: default scheme for middleware. NopAuthenticationScheme: used by CookieAuthenticationService (SignIn/SignOut/Authenticate).
            services.AddAuthentication("NopCookie")
                .AddCookie("NopCookie", options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.AccessDeniedPath = "/";
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                })
                .AddCookie("NopAuthenticationScheme", options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.AccessDeniedPath = "/";
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                });

            services.AddAuthorization();

            // Use Autofac for DI
            Console.WriteLine("[LOG] ConfigureApplicationServices: Configuring Autofac...");
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Load or create NopConfig
            // TODO: Load from appsettings.json or Web.config if available
            // For now, create a default configuration (IgnoreStartupTasks defaults to false)
            Console.WriteLine("[LOG] ConfigureApplicationServices: Creating NopConfig...");
            var nopConfig = new NopConfig();
            EngineContext.SetDefaultConfig(nopConfig);

            // Register all nopCommerce services via DependencyRegistrar pattern
            // We need to create the typeFinder and engine instance first, but we can't create the engine yet
            // because it needs the container. So we'll register ITypeFinder now, and IEngine later.
            Console.WriteLine("[LOG] ConfigureApplicationServices: Creating WebAppTypeFinder...");
            var typeFinder = new WebAppTypeFinder();

            builder.Host.ConfigureContainer<ContainerBuilder>((hostBuilderContext, containerBuilder) =>
            {
                // Register core infrastructure components first (before DependencyRegistrars)
                // These are needed by DependencyRegistrars and other services
                containerBuilder.RegisterInstance(nopConfig).As<NopConfig>().SingleInstance();
                containerBuilder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();

                // Find all DependencyRegistrar implementations
                Console.WriteLine("[LOG] ConfigureApplicationServices: Finding DependencyRegistrars...");
                var registrarTypes = typeFinder.FindClassesOfType<IDependencyRegistrar>();
                Console.WriteLine($"[LOG] ConfigureApplicationServices: Found {registrarTypes.Count()} DependencyRegistrars");

                var registrars = registrarTypes
                    .Select(type => (IDependencyRegistrar)Activator.CreateInstance(type))
                    .OrderBy(r => r.Order)
                    .ToList();

                // Register all services via DependencyRegistrars
                Console.WriteLine("[LOG] ConfigureApplicationServices: Registering services via DependencyRegistrars...");
                foreach (var registrar in registrars)
                {
                    Console.WriteLine($"[LOG] ConfigureApplicationServices: Registering {registrar.GetType().Name} (Order: {registrar.Order})");
                    registrar.Register(containerBuilder, typeFinder, nopConfig);
                }
                Console.WriteLine("[LOG] ConfigureApplicationServices: Service registration complete.");
            });
        }
    }
}
