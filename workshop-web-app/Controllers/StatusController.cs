using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;

namespace workshop_web_app.Controllers
{
    public class StatusController : Controller
    {
        private readonly StatusRepository _statusRepo;

        public StatusController(StatusRepository statusRepo)
        {
            _statusRepo = statusRepo;
        }

        public async Task<IActionResult> Index()
        {
            var statuses = await _statusRepo.GetAllStatusesAsync();
            return View(statuses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var status = await _statusRepo.GetStatusByIdAsync(id);
            if (status == null)
            {
                return NotFound();
            }
            return View(status);
        }
    }
}