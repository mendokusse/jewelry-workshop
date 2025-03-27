using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;


namespace workshop_web_app.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepo;

        public AccountController(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User model)
        {
            var users = await _userRepo.GetSearchUsersByEmailAsync(model.UserEmail);
            
            User foundUser = null;
            foreach (var user in users)
            {
                if (user.UserEmail == model.UserEmail && user.UserPasswordHash == model.UserPasswordHash)
                {
                    foundUser = user;
                    break;
                }
            }
            
            if (foundUser != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, foundUser.UserName),
                    new Claim("UserId", foundUser.UserId.ToString()),
                    new Claim(ClaimTypes.Role, foundUser.Role?.RoleName ?? "User")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return RedirectToAction("Index", "Account");
            }
            else
            {
                ModelState.AddModelError("", "Неверный адрес электронной почты или пароль.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            var existingUsers = await _userRepo.GetSearchUsersByEmailAsync(model.UserEmail);
            if (existingUsers != null && existingUsers.Count > 0)
            {
                ModelState.AddModelError("", "Пользователь с такой электронной почтой уже существует.");
                return View(model);
            }
            
            model.RoleId = 3;
            int newUserId = await _userRepo.AddUserAsync(model);
            
            if (newUserId > 0)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.UserName),
                    new Claim("UserId", newUserId.ToString()),
                    new Claim(ClaimTypes.Role, "Customer")
                };
                            
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                            
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                            
                return RedirectToAction("Index", "Home");
            }
            
            ModelState.AddModelError("", "Ошибка при регистрации пользователя.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}