using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;

namespace workshop_web_app.Controllers
{
    public class ProductTypeController : Controller
    {
        private readonly ProductTypeRepository _productTypeRepo;

        public ProductTypeController(ProductTypeRepository productTypeRepo)
        {
            _productTypeRepo = productTypeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var productTypes = await _productTypeRepo.GetAllProductTypesAsync();
            return View(productTypes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var productType = await _productTypeRepo.GetProductTypeByIdAsync(id);
            if (productType == null)
            {
                return NotFound();
            }
            return View(productType);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: /ProductType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductType productType)
        {
            if (ModelState.IsValid)
            {
                int newId = await _productTypeRepo.AddProductTypeAsync(productType);
                return RedirectToAction(nameof(Details), new { id = newId });
            }
            return View(productType);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var productType = await _productTypeRepo.GetProductTypeByIdAsync(id);
            if (productType == null)
            {
                return NotFound();
            }
            return View(productType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductType productType)
        {
            if (id != productType.ProductTypeId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                bool updated = await _productTypeRepo.UpdateProductTypeAsync(productType);
                if (updated)
                {
                    return RedirectToAction(nameof(Details), new { id = productType.ProductTypeId });
                }
                else
                {
                    return NotFound();
                }
            }
            return View(productType);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var productType = await _productTypeRepo.GetProductTypeByIdAsync(id);
            if (productType == null)
            {
                return NotFound();
            }
            return View(productType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _productTypeRepo.DeleteProductTypeAsync(id);
            if (deleted)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound();
            }
        }
    }
}