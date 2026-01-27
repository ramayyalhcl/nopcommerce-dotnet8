using Nop.Core.Plugins;

namespace Nop.Services.Common
{
    /// <summary>
    /// Misc plugin interface. 
    /// Updated for .NET 8 - configuration routing removed (handled by Web.Framework)
    /// Pattern based on official 4.70.5
    /// </summary>
    public partial interface IMiscPlugin : IPlugin
    {
        // GetConfigurationRoute removed - ASP.NET Core uses different plugin configuration approach
    }
}
