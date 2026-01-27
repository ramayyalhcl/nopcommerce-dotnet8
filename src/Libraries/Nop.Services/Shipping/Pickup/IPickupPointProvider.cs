// using System.Web.Routing; - Removed for .NET 8
using Nop.Core.Domain.Common;
using Nop.Core.Plugins;
using Nop.Services.Shipping.Tracking;

namespace Nop.Services.Shipping.Pickup
{
    /// <summary>
    /// Represents an interface of pickup point provider
    /// Updated for .NET 8 - routing removed per official 4.70.5 pattern
    /// </summary>
    public partial interface IPickupPointProvider : IPlugin
    {
        #region Properties

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        IShipmentTracker ShipmentTracker { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Get pickup points for the address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>Represents a response of getting pickup points</returns>
        GetPickupPointsResponse GetPickupPoints(Address address);

        // GetConfigurationRoute removed - ASP.NET Core uses different plugin configuration approach

        #endregion
    }
}
