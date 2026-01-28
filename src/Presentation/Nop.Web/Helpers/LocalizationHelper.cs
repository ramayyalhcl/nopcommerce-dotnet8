using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Web.Framework.Localization;

namespace Nop.Web.Helpers
{
    /// <summary>
    /// Localization helper for Razor views
    /// Provides T() function that can be called directly in views
    /// </summary>
    public static class LocalizationHelper
    {
        /// <summary>
        /// Get localized string (T helper for Razor views)
        /// </summary>
        public static LocalizedString T(string resourceName, string defaultValue = "", params object[] arguments)
        {
            try
            {
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                
                var resourceValue = localizationService.GetResource(resourceName, workContext.WorkingLanguage.Id, true, defaultValue);
                
                if (arguments != null && arguments.Length > 0)
                {
                    resourceValue = string.Format(resourceValue, arguments);
                }
                
                return new LocalizedString(resourceValue);
            }
            catch
            {
                return new LocalizedString(defaultValue ?? resourceName);
            }
        }
    }
}
