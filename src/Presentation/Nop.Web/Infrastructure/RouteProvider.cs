using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Web.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            // TODO: Convert all MapLocalizedRoute calls to MapControllerRoute with lang pattern
            // See official 4.70.5 Nop.Web/Infrastructure/RouteProvider.cs for reference pattern:
            // var lang = GetLanguageRoutePattern();
            // endpointRouteBuilder.MapControllerRoute(name: "RouteName", 
            //     pattern: $"{lang}/path/", 
            //     defaults: new { controller = "ControllerName", action = "ActionName" });
            // Stubbed for compilation - routing will be implemented when Program.cs is created
        }

        public int Priority => 0;
    }
}
