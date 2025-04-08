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

        [Route("Account/Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login");
            }
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return View("~/Views/Account/Edit.cshtml", user);
        }

        [Route("Account/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User formModel, string newPassword, string confirmPassword)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                            ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return RedirectToAction("~/Views/Account/Login.cshtml");
            }

            if (formModel.UserId != currentUserId)
            {
                return Forbid();
            }

            var originalUser = await _userRepo.GetUserByIdAsync(currentUserId);
            if (originalUser == null)
            {
                return RedirectToAction("~/Views/Account/Login.cshtml");
            }

            if (!string.IsNullOrEmpty(formModel.UserEmail) && formModel.UserEmail != originalUser.UserEmail)
            {
                originalUser.UserEmail = formModel.UserEmail;
            }

            if (!string.IsNullOrEmpty(formModel.UserName) && formModel.UserName != originalUser.UserName)
            {
                originalUser.UserName = formModel.UserName;
            }

            if (!string.IsNullOrEmpty(formModel.UserPhone) && formModel.UserPhone != originalUser.UserPhone)
            {
                originalUser.UserPhone = formModel.UserPhone;
            }

            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "Пароли не совпадают.");
                    return View("~/Views/Account/Edit.cshtml", originalUser);
                }
                originalUser.UserPasswordHash = newPassword;
            }

            bool updated = await _userRepo.UpdateUserAsync(originalUser);
            if (!updated)
            {
                ModelState.AddModelError("", "Ошибка при обновлении данных аккаунта.");
                return View("~/Views/Account/Edit.cshtml", originalUser);
            }
            return RedirectToAction("Index");
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