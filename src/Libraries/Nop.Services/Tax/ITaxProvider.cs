// using System.Web.Routing; - Removed for .NET 8
using Nop.Core.Plugins;

namespace Nop.Services.Tax
{
    /// <summary>
    /// Provides an interface for creating tax providers
    /// Updated for .NET 8 - routing removed per official 4.70.5 pattern
    /// </summary>
    public partial interface ITaxProvider : IPlugin
    {
        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest);

        // GetConfigurationRoute removed - ASP.NET Core uses different plugin configuration approach
    }
}
