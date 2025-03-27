using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Repositories;

namespace workshop_web_app.Controllers
{
    public class CatalogController : Controller
    {
        private readonly CatalogRepository _catalogRepo;
        private readonly ProductTypeRepository _productTypeRepo;

        public CatalogController(CatalogRepository catalogRepo, ProductTypeRepository productTypeRepo)
        {
            _catalogRepo = catalogRepo;
            _productTypeRepo = productTypeRepo;
        }

        public async Task<IActionResult> Index(decimal? minPrice, decimal? maxPrice, int? productTypeId)
        {
            var items = await _catalogRepo.GetCatalogItemsAsync(minPrice, maxPrice, productTypeId);
            var productTypes = await _productTypeRepo.GetAllProductTypesAsync();
            ViewBag.ProductTypes = productTypes;
            return View(items);
        }
    }
}