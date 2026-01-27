using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Nop.Web.Framework.Mvc.Routes
{
    /// <summary>
    /// GUID route constraint - updated for ASP.NET Core
    /// </summary>
    public class GuidConstraint : IRouteConstraint
    {
        private readonly bool _allowEmpty;

        public GuidConstraint(bool allowEmpty)
        {
            _allowEmpty = allowEmpty;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.ContainsKey(routeKey))
            {
                string stringValue = values[routeKey]?.ToString();

                if (!string.IsNullOrEmpty(stringValue))
                {
                    return Guid.TryParse(stringValue, out var guidValue) && 
                           (_allowEmpty || guidValue != Guid.Empty);
                }
            }

            return false;
        }
    }
}

