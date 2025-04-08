using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;
using System.Security.Claims;

namespace workshop_web_app.Controllers
{
    public class UserController : Controller
    {
        private readonly UserRepository _userRepo;

        public UserController(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [Route("Account")]
        [Route("Account/Index")]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return RedirectToAction("Login");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login");
            }
            
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View("~/Views/Account/Index.cshtml", user);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                int newUserId = await _userRepo.AddUserAsync(user);
                return RedirectToAction(nameof(Details), new { id = newUserId });
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                bool updated = await _userRepo.UpdateUserAsync(user);
                if (updated)
                {
                    return RedirectToAction(nameof(Details), new { id = user.UserId });
                }
                else
                {
                    return NotFound();
                }
            }
            return View(user);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _userRepo.DeleteUserByIdAsync(id);
            if (deleted)
            {
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }

        public async Task<IActionResult> Search(string userName)
        {
            var users = await _userRepo.GetSearchUsersByNameAsync(userName);
            return View("Index", users);
        }
    }
}