using Microsoft.AspNetCore.Routing;

namespace Nop.Web.Framework.Mvc.Routes
{
    /// <summary>
    /// Route provider interface - updated for ASP.NET Core endpoint routing
    /// </summary>
    public interface IRouteProvider
    {
        /// <summary>
        /// Register routes using endpoint routing
        /// </summary>
        /// <param name="endpointRouteBuilder">Endpoint route builder</param>
        void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder);

        /// <summary>
        /// Gets route provider priority
        /// </summary>
        int Priority { get; }
    }
}

