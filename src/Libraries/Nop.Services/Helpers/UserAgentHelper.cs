using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
// System.Web removed - not available in .NET 8
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;

namespace Nop.Services.Helpers
{
    /// <summary>
    /// User agent helper
    /// </summary>
    public partial class UserAgentHelper : IUserAgentHelper
    {
        private readonly NopConfig _config;
        // Removed HttpContext - will get user agent via injected IWebHelper instead
        private static readonly object _locker = new object();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="config">Config</param>
        public UserAgentHelper(NopConfig config)
        {
            this._config = config;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual BrowscapXmlHelper GetBrowscapXmlHelper()
        {
            if (Singleton<BrowscapXmlHelper>.Instance != null)
                return Singleton<BrowscapXmlHelper>.Instance;

            //no database created
            if (String.IsNullOrEmpty(_config.UserAgentStringsPath))
                return null;

            //prevent multi loading data
            lock (_locker)
            {
                //data can be loaded while we waited
                if (Singleton<BrowscapXmlHelper>.Instance != null)
                    return Singleton<BrowscapXmlHelper>.Instance;

                var userAgentStringsPath = CommonHelper.MapPath(_config.UserAgentStringsPath);
                var crawlerOnlyUserAgentStringsPath = string.IsNullOrEmpty(_config.CrawlerOnlyUserAgentStringsPath) ? string.Empty : CommonHelper.MapPath(_config.CrawlerOnlyUserAgentStringsPath);

                var browscapXmlHelper = new BrowscapXmlHelper(userAgentStringsPath, crawlerOnlyUserAgentStringsPath);
                Singleton<BrowscapXmlHelper>.Instance = browscapXmlHelper;

                return Singleton<BrowscapXmlHelper>.Instance;
            }
        }

        /// <summary>
        /// Get a value indicating whether the request is made by search engine (web crawler)
        /// </summary>
        /// <returns>Result</returns>
        public virtual bool IsSearchEngine()
        {
            // TODO: Reimplement using IWebHelper.GetUserAgent() or IHttpContextAccessor
            // HttpContext not available directly in class library
            return false;
        }
    }
}