using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Http;
using Nop.Core.Security;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Recently viewed products service - implemented based on official 4.70.5 pattern
    /// </summary>
    public partial class RecentlyViewedProductsService : IRecentlyViewedProductsService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CookieSettings _cookieSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public RecentlyViewedProductsService(
            CatalogSettings catalogSettings,
            CookieSettings cookieSettings,
            IHttpContextAccessor httpContextAccessor,
            IProductService productService,
            IWebHelper webHelper)
        {
            _catalogSettings = catalogSettings;
            _cookieSettings = cookieSettings;
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
            _webHelper = webHelper;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get a list of identifier of recently viewed products
        /// </summary>
        protected virtual List<int> GetRecentlyViewedProductsIds()
        {
            return GetRecentlyViewedProductsIds(int.MaxValue);
        }

        /// <summary>
        /// Get a list of identifier of recently viewed products
        /// </summary>
        protected virtual List<int> GetRecentlyViewedProductsIds(int number)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request == null)
                return new List<int>();

            var cookieName = $"{NopCookieDefaults.Prefix}{NopCookieDefaults.RecentlyViewedProductsCookie}";
            var cookieValue = httpContext.Request.Cookies[cookieName];

            if (string.IsNullOrEmpty(cookieValue))
                return new List<int>();

            return cookieValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .Take(number)
                .ToList();
        }

        #endregion

        #region Methods

        public virtual IList<Product> GetRecentlyViewedProducts(int number)
        {
            var products = new List<Product>();
            var productIds = GetRecentlyViewedProductsIds(number);
            
            foreach (var product in _productService.GetProductsByIds(productIds.ToArray()))
            {
                if (product.Published && !product.Deleted)
                    products.Add(product);
            }
            
            return products;
        }

        public virtual void AddProductToRecentlyViewedList(int productId)
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Response == null)
                return;

            var ids = GetRecentlyViewedProductsIds();
            if (!ids.Contains(productId))
                ids.Insert(0, productId);

            var maxProducts = _catalogSettings.RecentlyViewedProductsNumber;
            ids = ids.Take(maxProducts).ToList();

            var cookieName = $"{NopCookieDefaults.Prefix}{NopCookieDefaults.RecentlyViewedProductsCookie}";
            var cookieValue = string.Join(",", ids);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(365),
                HttpOnly = true,
                Secure = _cookieSettings.SecurePolicy == CookieSecurePolicy.Always,
                SameSite = _cookieSettings.SameSiteMode
            };

            httpContext.Response.Cookies.Append(cookieName, cookieValue, cookieOptions);
        }

        #endregion
    }
}

