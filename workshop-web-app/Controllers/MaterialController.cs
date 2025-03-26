using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using workshop_web_app.Models;
using workshop_web_app.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace workshop_web_app.Controllers
{
    [AllowAnonymous]
    public class MaterialController : Controller
    {
        private readonly MaterialRepository _materialRepo;

        public MaterialController(MaterialRepository materialRepo)
        {
            _materialRepo = materialRepo;
        }

        public async Task<IActionResult> Index()
        {
            var materials = await _materialRepo.GetAllMaterialsAsync();
            return View(materials);
        }

        public async Task<IActionResult> Details(int id)
        {
            var material = await _materialRepo.GetMaterialByIdAsync(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Material material)
        {
            if (ModelState.IsValid)
            {
                int newId = await _materialRepo.AddMaterialAsync(material);
                return RedirectToAction(nameof(Details), new { id = newId });
            }
            return View(material);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var material = await _materialRepo.GetMaterialByIdAsync(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Material material)
        {
            if (id != material.MaterialId)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                bool updated = await _materialRepo.UpdateMaterialAsync(material);
                if (updated)
                    return RedirectToAction(nameof(Details), new { id = material.MaterialId });
                else
                    return NotFound();
            }
            return View(material);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var material = await _materialRepo.GetMaterialByIdAsync(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _materialRepo.DeleteMaterialAsync(id);
            if (deleted)
                return RedirectToAction(nameof(Index));
            else
                return NotFound();
        }
    }
}