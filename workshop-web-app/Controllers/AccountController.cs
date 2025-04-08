using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace workshop_web_app.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepo;
        private readonly OrderRepository _orderRepo;

        public AccountController(UserRepository userRepo, OrderRepository orderRepo)
        {
            _userRepo = userRepo;
            _orderRepo = orderRepo;
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

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            // Получаем идентификатор текущего пользователя из клеймов
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Получаем все заказы (без деталей)
            var orders = await _orderRepo.GetAllOrdersAsync();
            // Фильтруем только заказы текущего пользователя (заказчика)
            var myOrders = orders.Where(o => o.CustomerUserId == userId).ToList();
            
            // Для каждого заказа получаем его полное представление (с деталями) вызовом GetOrderByIdAsync,
            // как это делается в админской панели
            for (int i = 0; i < myOrders.Count; i++)
            {
                myOrders[i] = await _orderRepo.GetOrderByIdAsync(myOrders[i].OrderId);
            }
            
            return View(myOrders);
        }

    }
}