using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;

namespace workshop_web_app.Controllers
{
    public class RoleController : Controller
    {
        private readonly RoleRepository _roleRepo;

        public RoleController(RoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleRepo.GetAllRolesAsync();
            return View(roles);
        }

        public async Task<IActionResult> Details(int id)
        {
            var role = await _roleRepo.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }
    }
}