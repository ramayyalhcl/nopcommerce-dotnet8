//Contributor:  Nicholas Mayne

using Nop.Core.Plugins;

namespace Nop.Services.Authentication.External
{
    /// <summary>
    /// Provides an interface for creating external authentication methods
    /// Updated for .NET 8 - routing methods removed per official 4.70.5 pattern
    /// </summary>
    public partial interface IExternalAuthenticationMethod : IPlugin
    {
        // GetConfigurationRoute removed - ASP.NET Core uses different plugin configuration approach
        // GetPublicInfoRoute removed - handled by Web.Framework
    }
}
