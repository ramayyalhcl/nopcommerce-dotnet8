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
            
            // Create view model with product and picture URL
            var model = products.Select(p => new
            {
                Product = p,
                PictureUrl = GetProductPictureUrl(p)
            }).ToList();
            
            _logger.LogInformation("HomeController.Index: Fetched {Count} products for homepage", products?.Count ?? 0);
            
            return View(model);
        }

        private string GetProductPictureUrl(Nop.Core.Domain.Catalog.Product product)
        {
            // .NET 8.0: Get first product picture URL (300px size for homepage)
            var productPictures = product.ProductPictures?.OrderBy(x => x.DisplayOrder).ToList();
            if (productPictures != null && productPictures.Any())
            {
                var firstPicture = productPictures.First();
                return _pictureService.GetPictureUrl(firstPicture.PictureId, targetSize: 300);
            }
            
            // Return default picture if product has no pictures
            return _pictureService.GetDefaultPictureUrl(targetSize: 300);
        }
    }
}
