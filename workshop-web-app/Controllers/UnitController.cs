using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;

namespace workshop_web_app.Controllers
{
    public class UnitController : Controller
    {
        private readonly UnitRepository _unitRepo;

        public UnitController(UnitRepository unitRepo)
        {
            _unitRepo = unitRepo;
        }

        public async Task<IActionResult> Index()
        {
            var units = await _unitRepo.GetAllUnitsAsync();
            return View(units);
        }

        public async Task<IActionResult> Details(int id)
        {
            var unit = await _unitRepo.GetUnitByIdAsync(id);
            if (unit == null)
            {
                return NotFound();
            }
            return View(unit);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Unit unit)
        {
            if (ModelState.IsValid)
            {
                int newId = await _unitRepo.AddUnitAsync(unit);
                return RedirectToAction(nameof(Details), new { id = newId });
            }
            return View(unit);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var unit = await _unitRepo.GetUnitByIdAsync(id);
            if (unit == null)
            {
                return NotFound();
            }
            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Unit unit)
        {
            if (id != unit.UnitId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                bool updated = await _unitRepo.UpdateUnitAsync(unit);
                if (updated)
                {
                    return RedirectToAction(nameof(Details), new { id = unit.UnitId });
                }
                else
                {
                    return NotFound();
                }
            }
            return View(unit);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var unit = await _unitRepo.GetUnitByIdAsync(id);
            if (unit == null)
            {
                return NotFound();
            }
            return View(unit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _unitRepo.DeleteUnitAsync(id);
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