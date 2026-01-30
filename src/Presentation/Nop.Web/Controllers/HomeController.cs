using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Security;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;

namespace Nop.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        // .NET 8.0: Added DI for ProductService, CategoryService, and PictureService
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IProductService productService,
            ICategoryService categoryService,
            IPictureService pictureService,
            ILogger<HomeController> _logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _pictureService = pictureService;
            this._logger = _logger;
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult Index()
        {
            // .NET 8.0: Fetch products and categories with pictures from database
            _logger.LogDebug("HomeController.Index: Fetching homepage data from database");
            
            // Fetch products
            var products = _productService.GetAllProductsDisplayedOnHomePage();
            
            // Store product picture URLs in ViewBag
            var pictureUrls = new Dictionary<int, string>();
            foreach (var product in products)
            {
                pictureUrls[product.Id] = GetProductPictureUrl(product);
            }
            ViewBag.PictureUrls = pictureUrls;
            
            // Fetch categories displayed on homepage
            var categories = _categoryService.GetAllCategoriesDisplayedOnHomePage();
            
            // Store category picture URLs in ViewBag
            var categoryPictureUrls = new Dictionary<int, string>();
            foreach (var category in categories)
            {
                categoryPictureUrls[category.Id] = GetCategoryPictureUrl(category);
            }
            ViewBag.CategoryPictureUrls = categoryPictureUrls;
            ViewBag.Categories = categories;
            
            _logger.LogInformation("HomeController.Index: Fetched {ProductCount} products and {CategoryCount} categories for homepage", 
                products?.Count ?? 0, categories?.Count ?? 0);
            
            return View(products);
        }

        private string GetProductPictureUrl(Nop.Core.Domain.Catalog.Product product)
        {
            // .NET 8.0: Get first product picture URL (300px size for homepage)
            var productPictures = product.ProductPictures?.OrderBy(x => x.DisplayOrder).ToList();
            
            _logger.LogDebug("Product '{ProductName}' (ID: {ProductId}) has {PictureCount} pictures", 
                product.Name, product.Id, productPictures?.Count ?? 0);
            
            if (productPictures != null && productPictures.Any())
            {
                var firstPicture = productPictures.First();
                _logger.LogDebug("  -> Using PictureId: {PictureId}, DisplayOrder: {DisplayOrder}", 
                    firstPicture.PictureId, firstPicture.DisplayOrder);
                var url = _pictureService.GetPictureUrl(firstPicture.PictureId, targetSize: 300);
                _logger.LogDebug("  -> Generated URL: {Url}", url);
                return url;
            }
            
            // Return default picture if product has no pictures
            _logger.LogWarning("Product '{ProductName}' (ID: {ProductId}) has NO pictures - using default", 
                product.Name, product.Id);
            return _pictureService.GetDefaultPictureUrl(targetSize: 300);
        }

        private string GetCategoryPictureUrl(Nop.Core.Domain.Catalog.Category category)
        {
            // .NET 8.0: Get category picture URL (300px size for homepage)
            _logger.LogDebug("Category '{CategoryName}' (ID: {CategoryId}) PictureId: {PictureId}", 
                category.Name, category.Id, category.PictureId);
            
            if (category.PictureId > 0)
            {
                var url = _pictureService.GetPictureUrl(category.PictureId, targetSize: 300);
                _logger.LogDebug("  -> Generated URL: {Url}", url);
                return url;
            }
            
            // Return default picture if category has no picture
            _logger.LogWarning("Category '{CategoryName}' (ID: {CategoryId}) has NO picture - using default", 
                category.Name, category.Id);
            return _pictureService.GetDefaultPictureUrl(targetSize: 300);
        }
    }
}
