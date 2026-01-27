using System.Collections.Generic;
using Nop.Core.Plugins;

namespace Nop.Services.Cms
{
    /// <summary>
    /// Provides an interface for creating widgets
    /// Updated to match .NET 8 pattern from official 4.70.5
    /// </summary>
    public partial interface IWidgetPlugin : IPlugin
    {
        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        bool HideInWidgetList { get; }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        IList<string> GetWidgetZones();

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        string GetWidgetViewComponentName(string widgetZone);
    }
}

