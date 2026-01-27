using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Services.Stores;

namespace Nop.Web.Framework
{
    /// <summary>
    /// Store context for web application
    /// </summary>
    public partial class WebStoreContext : IStoreContext
    {
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private Store _cachedStore;

        public WebStoreContext(IStoreService storeService, IWebHelper webHelper, IHttpContextAccessor httpContextAccessor)
        {
            this._storeService = storeService;
            this._webHelper = webHelper;
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets or sets the current store
        /// </summary>
        public virtual Store CurrentStore
        {
            get
            {
                if (_cachedStore != null)
                    return _cachedStore;

                //try to determine the current store by HTTP_HOST
                var host = _httpContextAccessor?.HttpContext?.Request.Host.Host ?? "localhost";
                var allStores = _storeService.GetAllStores();
                var store = allStores.FirstOrDefault(s => s.ContainsHostValue(host));

                if (store == null)
                {
                    //load the first found store
                    store = allStores.FirstOrDefault();
                }
                if (store == null)
                    throw new Exception("No store could be loaded");

                _cachedStore = store;
                return _cachedStore;
            }
        }
    }
}
