using Microsoft.AspNetCore.Routing;

namespace Nop.Web.Framework.Mvc.Routes
{
    /// <summary>
    /// Route publisher - updated for ASP.NET Core endpoint routing
    /// </summary>
    public interface IRoutePublisher
    {
        /// <summary>
        /// Register routes using endpoint routing
        /// </summary>
        /// <param name="endpointRouteBuilder">Endpoint route builder</param>
        void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder);
    }
}

