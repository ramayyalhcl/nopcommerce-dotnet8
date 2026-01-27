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
    /// Compare products service - implemented based on official 4.70.5 pattern
    /// </summary>
    public partial class CompareProductsService : ICompareProductsService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CookieSettings _cookieSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public CompareProductsService(
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
        /// Get a list of identifier of compared products
        /// </summary>
        protected virtual List<int> GetComparedProductIds()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request == null)
                return new List<int>();

            var cookieName = $"{NopCookieDefaults.Prefix}{NopCookieDefaults.CompareProductsCookie}";
            var cookieValue = httpContext.Request.Cookies[cookieName];

            if (string.IsNullOrEmpty(cookieValue))
                return new List<int>();

            return cookieValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        #endregion

        #region Methods

        public virtual IList<Product> GetComparedProducts()
        {
            var products = new List<Product>();
            var productIds = GetComparedProductIds();
            
            foreach (var productId in productIds)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.Published && !product.Deleted)
                    products.Add(product);
            }
            
            return products;
        }

        public virtual void RemoveProductFromCompareList(int productId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Response == null)
                return;

            var ids = GetComparedProductIds();
            ids.Remove(productId);

            var cookieName = $"{NopCookieDefaults.Prefix}{NopCookieDefaults.CompareProductsCookie}";
            var cookieValue = string.Join(",", ids);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(10),
                HttpOnly = true,
                Secure = _cookieSettings.SecurePolicy == CookieSecurePolicy.Always,
                SameSite = _cookieSettings.SameSiteMode
            };

            httpContext.Response.Cookies.Append(cookieName, cookieValue, cookieOptions);
        }

        public virtual void AddProductToCompareList(int productId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Response == null)
                return;

            var ids = GetComparedProductIds();
            if (!ids.Contains(productId))
                ids.Insert(0, productId);

            var maxProducts = _catalogSettings.CompareProductsNumber;
            ids = ids.Take(maxProducts).ToList();

            var cookieName = $"{NopCookieDefaults.Prefix}{NopCookieDefaults.CompareProductsCookie}";
            var cookieValue = string.Join(",", ids);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(10),
                HttpOnly = true,
                Secure = _cookieSettings.SecurePolicy == CookieSecurePolicy.Always,
                SameSite = _cookieSettings.SameSiteMode
            };

            httpContext.Response.Cookies.Append(cookieName, cookieValue, cookieOptions);
        }

        public virtual void ClearCompareProducts()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Response == null)
                return;

            var cookieName = $"{NopCookieDefaults.Prefix}{NopCookieDefaults.CompareProductsCookie}";
            httpContext.Response.Cookies.Delete(cookieName);
        }

        #endregion
    }
}

