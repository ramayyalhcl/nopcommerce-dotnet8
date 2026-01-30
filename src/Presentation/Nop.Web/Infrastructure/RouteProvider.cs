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

            // Register standard MVC route pattern
            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Registering default route (pattern: '{controller=Home}/{action=Index}/{id?}')");
            endpointRouteBuilder.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            
            System.Console.WriteLine("[LOG] RouteProvider.RegisterRoutes: Route registration complete.");
        }

        public int Priority => 0;
    }
}
