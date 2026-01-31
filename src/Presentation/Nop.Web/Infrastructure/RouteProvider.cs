using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Web.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Starting route registration...");
            
            // Register default route for Home/Index
            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Registering HomePage route (pattern: '')");
            endpointRouteBuilder.MapControllerRoute(
                name: "HomePage",
                pattern: "",
                defaults: new { controller = "Home", action = "Index" });

            // Register Install controller explicitly so /Install and /Install/Index are always reachable
            // (avoids 404 when DB not installed and form posts to Install)
            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Registering Install route (pattern: 'Install/{action=Index}/{id?}')");
            endpointRouteBuilder.MapControllerRoute(
                name: "Install",
                pattern: "Install/{action=Index}/{id?}",
                defaults: new { controller = "Install", action = "Index" });

            // .NET 8.0: Register explicit catalog routes for numeric IDs (e.g., /category/5, /product/10)
            // These MUST come before the default route to prevent /category/5 from routing to CategoryController (which doesn't exist)
            // IMPORTANT: Parameter names must match the action method signatures (categoryId, productId, manufacturerId)
            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Registering Category route (pattern: 'category/{categoryId}')");
            endpointRouteBuilder.MapControllerRoute(
                name: "Category",
                pattern: "category/{categoryId}",
                defaults: new { controller = "Catalog", action = "Category" },
                constraints: new { categoryId = @"\d+" });

            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Registering Product route (pattern: 'product/{productId}')");
            endpointRouteBuilder.MapControllerRoute(
                name: "Product",
                pattern: "product/{productId}",
                defaults: new { controller = "Product", action = "ProductDetails" },
                constraints: new { productId = @"\d+" });

            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Registering Manufacturer route (pattern: 'manufacturer/{manufacturerId}')");
            endpointRouteBuilder.MapControllerRoute(
                name: "Manufacturer",
                pattern: "manufacturer/{manufacturerId}",
                defaults: new { controller = "Catalog", action = "Manufacturer" },
                constraints: new { manufacturerId = @"\d+" });

            // Register standard MVC route pattern BEFORE slug route
            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Registering default route (pattern: '{controller=Home}/{action=Index}/{id?}')");
            endpointRouteBuilder.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // .NET 8.0: Register SEO-friendly slug route LAST (dynamic transformer)
            // Migrated from: GenericPathRoute.cs (3.90 .NET 4.5.1)
            // This handles URLs like /electronics, /build-your-own-computer
            // MUST be registered LAST so conventional routes are matched first
            // ONLY register if database is installed (avoids DI resolution errors during installation)
            if (Nop.Core.Data.DataSettingsHelper.DatabaseIsInstalled())
            {
                System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Registering slug route (pattern: '{**SeName}') - LAST");
                endpointRouteBuilder.MapDynamicControllerRoute<Nop.Web.Framework.Mvc.Routing.SlugRouteTransformer>(
                    "{**SeName}");
            }
            else
            {
                System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Skipping slug route (database not installed)");
            }
            
            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Route registration complete.");
        }

        public int Priority => 0;
    }
}
