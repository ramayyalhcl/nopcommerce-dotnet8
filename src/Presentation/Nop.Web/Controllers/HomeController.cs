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
        // .NET 8.0: Added DI for ProductService and PictureService
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IProductService productService,
            IPictureService pictureService,
            ILogger<HomeController> _logger)
        {
            _productService = productService;
            _pictureService = pictureService;
            this._logger = _logger;
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult Index()
        {
            // .NET 8.0: Fetch products with pictures from database
            _logger.LogDebug("HomeController.Index: Fetching homepage products from database");
            
            var products = _productService.GetAllProductsDisplayedOnHomePage();
            
            // Store picture URLs in ViewBag for easy access in view
            var pictureUrls = new Dictionary<int, string>();
            foreach (var product in products)
            {
                pictureUrls[product.Id] = GetProductPictureUrl(product);
            }
            ViewBag.PictureUrls = pictureUrls;
            
            _logger.LogInformation("HomeController.Index: Fetched {Count} products for homepage", products?.Count ?? 0);
            
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
    }
}
