using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
// using System.Web.Routing; - Removed for .NET 8
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;

namespace Nop.Web.Framework.Mvc.Routes
{
    /// <summary>
    /// Route publisher
    /// </summary>
    public class RoutePublisher : IRoutePublisher
    {
        protected readonly ITypeFinder typeFinder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="typeFinder"></param>
        public RoutePublisher(ITypeFinder typeFinder)
        {
            this.typeFinder = typeFinder;
        }

        /// <summary>
        /// Find a plugin descriptor by some type which is located into its assembly
        /// </summary>
        /// <param name="providerType">Provider type</param>
        /// <returns>Plugin descriptor</returns>
        protected virtual PluginDescriptor FindPlugin(Type providerType)
        {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            // PluginManager.ReferencedPlugins may be null if plugins aren't initialized yet
            // In that case, the route provider is not a plugin, so return null
            if (PluginManager.ReferencedPlugins == null)
                return null;

            foreach (var plugin in PluginManager.ReferencedPlugins)
            {
                if (plugin?.ReferencedAssembly == null)
                    continue;

                if (plugin.ReferencedAssembly.FullName == providerType.Assembly.FullName)
                    return plugin;
            }

            return null;
        }

        /// <summary>
        /// Register routes using endpoint routing
        /// </summary>
        /// <param name="endpointRouteBuilder">Endpoint route builder</param>
        public virtual void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            System.Console.WriteLine("[LOG] RoutePublisher.RegisterRoutes: Starting route discovery...");
            var routeProviderTypes = typeFinder.FindClassesOfType<IRouteProvider>();
            System.Console.WriteLine($"[LOG] RoutePublisher.RegisterRoutes: Found {routeProviderTypes.Count()} IRouteProvider implementations");
            
            var routeProviders = new List<IRouteProvider>();
            foreach (var providerType in routeProviderTypes)
            {
                System.Console.WriteLine($"[LOG] RoutePublisher.RegisterRoutes: Processing {providerType.Name}...");
                //Ignore not installed plugins
                var plugin = FindPlugin(providerType);
                if (plugin != null && !plugin.Installed)
                {
                    System.Console.WriteLine($"[LOG] RoutePublisher.RegisterRoutes: Skipping {providerType.Name} (plugin not installed)");
                    continue;
                }

                var provider = Activator.CreateInstance(providerType) as IRouteProvider;
                if (provider != null)
                {
                    routeProviders.Add(provider);
                    System.Console.WriteLine($"[LOG] RoutePublisher.RegisterRoutes: Added {providerType.Name} (Priority: {provider.Priority})");
                }
            }
            routeProviders = routeProviders.OrderByDescending(rp => rp.Priority).ToList();
            System.Console.WriteLine($"[LOG] RoutePublisher.RegisterRoutes: Calling RegisterRoutes on {routeProviders.Count} providers...");
            routeProviders.ForEach(rp => 
            {
                System.Console.WriteLine($"[LOG] RoutePublisher.RegisterRoutes: Calling RegisterRoutes on {rp.GetType().Name}...");
                rp.RegisterRoutes(endpointRouteBuilder);
            });
            System.Console.WriteLine("[LOG] RoutePublisher.RegisterRoutes: Route registration complete.");
        }
    }
}
