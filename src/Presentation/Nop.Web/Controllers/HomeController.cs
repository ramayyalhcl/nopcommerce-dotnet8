using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Security;
using Nop.Services.Catalog;
using Microsoft.Extensions.Logging;

namespace Nop.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        // .NET 8.0: Added DI for ProductService to fetch dynamic data
        private readonly IProductService _productService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IProductService productService,
            ILogger<HomeController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult Index()
        {
            // .NET 8.0: Fetch products from database dynamically
            _logger.LogDebug("HomeController.Index: Fetching homepage products from database");
            
            var products = _productService.GetAllProductsDisplayedOnHomePage();
            
            _logger.LogInformation("HomeController.Index: Fetched {Count} products for homepage", products?.Count ?? 0);
            
            return View(products);
        }
    }
}
