using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Seo;
using Nop.Services.Seo;
using System;
using System.Threading.Tasks;

namespace Nop.Web.Framework.Mvc.Routing
{
    /// <summary>
    /// .NET 8.0: Slug route transformer for SEO-friendly URLs
    /// Migrated from GenericPathRoute.cs (nopCommerce 3.90 .NET 4.5.1)
    /// 
    /// Purpose: Resolves SEO URLs like /electronics to Catalog/Category/{id}
    /// Legacy: Used System.Web.Routing.Route.GetRouteData()
    /// .NET 8.0: Uses DynamicRouteValueTransformer.TransformAsync()
    /// </summary>
    public partial class SlugRouteTransformer : DynamicRouteValueTransformer
    {
        #region Fields

        private readonly IUrlRecordService _urlRecordService;
        private readonly ILogger<SlugRouteTransformer> _logger;

        #endregion

        #region Constructor

        public SlugRouteTransformer(
            IUrlRecordService urlRecordService,
            ILogger<SlugRouteTransformer> logger)
        {
            _urlRecordService = urlRecordService;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// .NET 8.0: Transform route values based on SEO slug
        /// Replaces: GenericPathRoute.GetRouteData() from 3.90
        /// </summary>
        public override ValueTask<RouteValueDictionary> TransformAsync(
            HttpContext httpContext, 
            RouteValueDictionary values)
        {
            // Log entry for debugging
            _logger.LogDebug("SlugRouteTransformer: Processing request path={Path}", httpContext.Request.Path);

            // Get slug from route values (pattern: {**SeName})
            if (!values.TryGetValue("SeName", out var slugValue) || slugValue == null)
            {
                _logger.LogDebug("SlugRouteTransformer: No SeName in route values");
                return new ValueTask<RouteValueDictionary>(values);
            }

            var slug = slugValue.ToString();
            _logger.LogDebug("SlugRouteTransformer: Looking up slug='{Slug}'", slug);

            // Lookup URL record in database
            // Note: 3.90 methods are synchronous (not async)
            var urlRecord = _urlRecordService.GetBySlugCached(slug);
            
            if (urlRecord == null)
            {
                _logger.LogWarning("SlugRouteTransformer: No UrlRecord found for slug='{Slug}' -> PageNotFound", slug);
                
                // No URL record found - route to PageNotFound
                values["controller"] = "Common";
                values["action"] = "PageNotFound";
                return new ValueTask<RouteValueDictionary>(values);
            }

            _logger.LogDebug("SlugRouteTransformer: Found UrlRecord. EntityName={EntityName}, EntityId={EntityId}, IsActive={IsActive}",
                urlRecord.EntityName, urlRecord.EntityId, urlRecord.IsActive);

            // Check if URL record is active
            if (!urlRecord.IsActive)
            {
                // Find active slug for redirect
                var activeSlug = _urlRecordService.GetActiveSlug(
                    urlRecord.EntityId, 
                    urlRecord.EntityName, 
                    urlRecord.LanguageId);
                
                if (string.IsNullOrWhiteSpace(activeSlug))
                {
                    _logger.LogWarning("SlugRouteTransformer: Inactive UrlRecord with no active slug -> PageNotFound");
                    values["controller"] = "Common";
                    values["action"] = "PageNotFound";
                    return new ValueTask<RouteValueDictionary>(values);
                }

                // Redirect to active slug (permanent redirect)
                _logger.LogInformation("SlugRouteTransformer: Redirecting from inactive slug='{OldSlug}' to active slug='{NewSlug}'",
                    slug, activeSlug);
                
                // TODO: Implement redirect logic (Phase 2)
                // For now, route to the active slug
                slug = activeSlug;
            }

            // Map entity type to controller/action
            // This logic is ported from GenericPathRoute.GetRouteData() (3.90)
            switch (urlRecord.EntityName.ToLowerInvariant())
            {
                case "product":
                    _logger.LogDebug("SlugRouteTransformer: Routing to Product/ProductDetails, productId={ProductId}",
                        urlRecord.EntityId);
                    values["controller"] = "Product";
                    values["action"] = "ProductDetails";
                    values["productId"] = urlRecord.EntityId;  // .NET 8.0: Fixed to match ProductDetails(int productId) parameter
                    values["SeName"] = urlRecord.Slug;
                    break;

                case "category":
                    _logger.LogDebug("SlugRouteTransformer: Routing to Catalog/Category, categoryId={CategoryId}",
                        urlRecord.EntityId);
                    values["controller"] = "Catalog";
                    values["action"] = "Category";
                    values["categoryid"] = urlRecord.EntityId;  // .NET 8.0: Route values auto-convert int to object
                    values["SeName"] = urlRecord.Slug;
                    break;

                case "manufacturer":
                    _logger.LogDebug("SlugRouteTransformer: Routing to Catalog/Manufacturer, manufacturerId={ManufacturerId}",
                        urlRecord.EntityId);
                    values["controller"] = "Catalog";
                    values["action"] = "Manufacturer";
                    values["manufacturerid"] = urlRecord.EntityId;  // .NET 8.0: Route values auto-convert int to object
                    values["SeName"] = urlRecord.Slug;
                    break;

                case "vendor":
                    _logger.LogDebug("SlugRouteTransformer: Routing to Catalog/Vendor, vendorId={VendorId}",
                        urlRecord.EntityId);
                    values["controller"] = "Catalog";
                    values["action"] = "Vendor";
                    values["vendorid"] = urlRecord.EntityId;  // .NET 8.0: Route values auto-convert int to object
                    values["SeName"] = urlRecord.Slug;
                    break;

                case "newsitem":
                    _logger.LogDebug("SlugRouteTransformer: Routing to News/NewsItem, newsItemId={NewsItemId}",
                        urlRecord.EntityId);
                    values["controller"] = "News";
                    values["action"] = "NewsItem";
                    values["newsItemId"] = urlRecord.EntityId;  // .NET 8.0: Route values auto-convert int to object
                    values["SeName"] = urlRecord.Slug;
                    break;

                case "blogpost":
                    _logger.LogDebug("SlugRouteTransformer: Routing to Blog/BlogPost, blogPostId={BlogPostId}",
                        urlRecord.EntityId);
                    values["controller"] = "Blog";
                    values["action"] = "BlogPost";
                    values["blogPostId"] = urlRecord.EntityId;  // .NET 8.0: Route values auto-convert int to object
                    values["SeName"] = urlRecord.Slug;
                    break;

                case "topic":
                    _logger.LogDebug("SlugRouteTransformer: Routing to Topic/TopicDetails, topicId={TopicId}",
                        urlRecord.EntityId);
                    values["controller"] = "Topic";
                    values["action"] = "TopicDetails";
                    values["topicId"] = urlRecord.EntityId;  // .NET 8.0: Route values auto-convert int to object
                    values["SeName"] = urlRecord.Slug;
                    break;

                default:
                    _logger.LogWarning("SlugRouteTransformer: Unknown EntityName='{EntityName}' -> PageNotFound",
                        urlRecord.EntityName);
                    values["controller"] = "Common";
                    values["action"] = "PageNotFound";
                    break;
            }

            return new ValueTask<RouteValueDictionary>(values);
        }

        #endregion
    }
}
