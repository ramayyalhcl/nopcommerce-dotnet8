using System;
using Nop.Core;

namespace Nop.Data
{
    public static class Extensions
    {
        /// <summary>
        /// Get unproxied entity type
        /// </summary>
        /// <remarks> If your Entity Framework context is proxy-enabled, 
        /// the runtime will create a proxy instance of your entities, 
        /// i.e. a dynamically generated class which inherits from your entity class 
        /// and overrides its virtual properties by inserting specific code useful for example 
        /// for tracking changes and lazy loading.
        /// </remarks>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Type GetUnproxiedEntityType(this BaseEntity entity)
        {
            // ObjectContext doesn't exist in EF Core
            // EF Core proxies work differently - get the actual type
            var type = entity.GetType();
            
            // If it's a proxy, get the base type
            if (type.Namespace == "Castle.Proxies" || type.FullName.Contains("Proxy"))
                return type.BaseType;
                
            return type;
        }
    }
}
