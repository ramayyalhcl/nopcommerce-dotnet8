//Contributor:  Nicholas Mayne

using System.Collections.Generic;
// using System.Web; - Removed for ASP.NET Core
using Nop.Core.Infrastructure;

namespace Nop.Services.Authentication.External
{
    /// <summary>
    /// External authorizer helper
    /// </summary>
    public static partial class ExternalAuthorizerHelper
    {
        // TODO: Reimplement for ASP.NET Core using IHttpContextAccessor + ISession
        // HttpSessionStateBase doesn't exist in ASP.NET Core
        
        /* OLD ASP.NET FRAMEWORK CODE
        private static HttpSessionStateBase GetSession()
        {
            var session = EngineContext.Current.Resolve<HttpSessionStateBase>();
            return session;
        }
        */

        public static void StoreParametersForRoundTrip(OpenAuthenticationParameters parameters)
        {
            // TODO: Use ASP.NET Core ISession
            // Temporarily disabled - needs IHttpContextAccessor
        }
        
        public static OpenAuthenticationParameters RetrieveParametersFromRoundTrip(bool removeOnRetrieval)
        {
            // TODO: Use ASP.NET Core ISession
            return null; // Temporarily return null
        }

        public static void RemoveParameters()
        {
            // TODO: Use ASP.NET Core ISession
            // No-op for now
        }

        public static void AddErrorsToDisplay(string error)
        {
            // TODO: Implement using ASP.NET Core ISession
        }

        public static IList<string> RetrieveErrorsToDisplay(bool removeOnRetrieval)
        {
            // TODO: Implement using ASP.NET Core ISession
            return new List<string>();
        }
    }
}