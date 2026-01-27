using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;

namespace Nop.Core
{
    /// <summary>
    /// Represents a web helper - implemented based on official 4.70.5 pattern
    /// </summary>
    public partial class WebHelper : IWebHelper
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly Lazy<IStoreContext> _storeContext;

        #endregion

        #region Ctor

        public WebHelper(
            IActionContextAccessor actionContextAccessor,
            IHostApplicationLifetime hostApplicationLifetime,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            Lazy<IStoreContext> storeContext)
        {
            _actionContextAccessor = actionContextAccessor;
            _hostApplicationLifetime = hostApplicationLifetime;
            _httpContextAccessor = httpContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _storeContext = storeContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check whether current HTTP request is available
        /// </summary>
        protected virtual bool IsRequestAvailable()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return false;

            try
            {
                if (_httpContextAccessor.HttpContext?.Request == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Methods

        public virtual string GetUrlReferrer()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            return _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        }

        public virtual string GetCurrentIpAddress()
        {
            if (!IsRequestAvailable() || _httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null)
                return string.Empty;

            var remoteIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            return (remoteIp.Equals(IPAddress.IPv6Loopback) ? IPAddress.Loopback : remoteIp).ToString();
        }

        public virtual string GetThisPageUrl(bool includeQueryString, bool? useSsl = null, bool lowercaseUrl = false)
        {
            if (!IsRequestAvailable())
                return string.Empty;

            var url = _httpContextAccessor.HttpContext.Request.GetDisplayUrl();
            
            if (!includeQueryString)
            {
                var questionMarkIndex = url.IndexOf('?');
                if (questionMarkIndex >= 0)
                    url = url.Substring(0, questionMarkIndex);
            }

            if (lowercaseUrl)
                url = url.ToLowerInvariant();

            return url;
        }

        public virtual bool IsCurrentConnectionSecured()
        {
            if (!IsRequestAvailable())
                return false;

            return _httpContextAccessor.HttpContext.Request.IsHttps;
        }

        public virtual string GetStoreHost(bool useSsl)
        {
            if (!IsRequestAvailable())
                return string.Empty;

            var host = _httpContextAccessor.HttpContext.Request.Host.Value;
            return host;
        }

        public virtual string GetStoreLocation(bool? useSsl = null)
        {
            var storeLocation = string.Empty;

            if (!IsRequestAvailable())
                return storeLocation;

            var request = _httpContextAccessor.HttpContext.Request;
            storeLocation = $"{request.Scheme}://{request.Host.Value}{request.PathBase.Value}";

            return storeLocation.TrimEnd('/');
        }

        public virtual bool IsStaticResource()
        {
            if (!IsRequestAvailable())
                return false;

            var path = _httpContextAccessor.HttpContext.Request.Path.Value ?? string.Empty;
            var staticFileExtensions = new[] { ".css", ".js", ".jpg", ".jpeg", ".png", ".gif", ".ico", ".svg", ".woff", ".woff2", ".ttf", ".eot" };

            foreach (var ext in staticFileExtensions)
            {
                if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public virtual string ModifyQueryString(string url, string queryStringModification, string anchor)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (string.IsNullOrEmpty(queryStringModification) && string.IsNullOrEmpty(anchor))
                return url;

            var urlParts = url.Split(new[] { '#' }, 2);
            var baseUrl = urlParts[0];
            var fragment = urlParts.Length > 1 ? urlParts[1] : string.Empty;

            var parts = baseUrl.Split(new[] { '?' }, 2);
            var currentQueryString = parts.Length > 1 ? parts[1] : string.Empty;

            var queryString = string.IsNullOrEmpty(queryStringModification) 
                ? currentQueryString 
                : queryStringModification;

            var result = parts[0];
            if (!string.IsNullOrEmpty(queryString))
                result += "?" + queryString;

            if (!string.IsNullOrEmpty(anchor))
                result += "#" + anchor;
            else if (!string.IsNullOrEmpty(fragment))
                result += "#" + fragment;

            return result;
        }

        public virtual string RemoveQueryString(string url, string queryString)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(queryString))
                return url;

            var parts = url.Split(new[] { '?' }, 2);
            if (parts.Length <= 1)
                return url;

            var qs = parts[1];
            var parameters = qs.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            var result = parts[0];
            var first = true;

            foreach (var param in parameters)
            {
                if (!param.StartsWith(queryString + "=", StringComparison.OrdinalIgnoreCase))
                {
                    result += first ? "?" : "&";
                    result += param;
                    first = false;
                }
            }

            return result;
        }

        public virtual T QueryString<T>(string name)
        {
            if (!IsRequestAvailable())
                return default(T);

            var queryValue = _httpContextAccessor.HttpContext.Request.Query[name].ToString();
            
            if (string.IsNullOrEmpty(queryValue))
                return default(T);

            try
            {
                return (T)Convert.ChangeType(queryValue, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public virtual void RestartAppDomain(bool makeRedirect = false, string redirectUrl = "")
        {
            // In ASP.NET Core, application restart is handled differently
            // Use IHostApplicationLifetime to stop the application
            _hostApplicationLifetime?.StopApplication();
        }

        public virtual bool IsRequestBeingRedirected
        {
            get
            {
                if (!IsRequestAvailable())
                    return false;

                var response = _httpContextAccessor.HttpContext.Response;
                return response.StatusCode >= 300 && response.StatusCode < 400;
            }
        }

        public virtual bool IsPostBeingDone
        {
            get
            {
                if (!IsRequestAvailable())
                    return false;

                return string.Equals(_httpContextAccessor.HttpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase);
            }
            set
            {
                // Setter kept for compatibility but cannot modify request method in ASP.NET Core
            }
        }

        #endregion
    }
}

