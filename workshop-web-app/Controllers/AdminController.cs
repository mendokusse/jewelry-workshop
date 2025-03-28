using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;

namespace workshop_web_app.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly OrderRepository _orderRepo;
        private readonly MaterialRepository _materialRepo;
        private readonly UserRepository _userRepo;

        public AdminController(OrderRepository orderRepo, MaterialRepository materialRepo, UserRepository userRepo)
        {
            _orderRepo = orderRepo;
            _materialRepo = materialRepo;
            _userRepo = userRepo;
        }

        [Authorize(Roles = "Admin,Jeweler,Manager,Accountant")]
        public async Task<IActionResult> Index() {
            return View();
        }

        // ******************************
        // 1. Раздел "Заказы"
        // ******************************

        // Просмотр списка заказов (доступен для Admin, Jeweler, Manager)
        [Authorize(Roles = "Admin,Jeweler,Manager")]
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderRepo.GetAllOrdersAsync();
            return View("Orders/Index", orders);
        }

        // Редактирование заказа (доступно только для Admin и Manager)
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();
            return View(order);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(int id, Order order)
        {
            if (id != order.OrderId)
                return BadRequest();
            if (await _orderRepo.UpdateOrderAsync(order))
                return RedirectToAction("Orders");
            return View(order);
        }

        // ******************************
        // 2. Раздел "Материалы"
        // ******************************

        // Просмотр списка материалов (доступен для Admin, Jeweler, Manager)
        [Authorize(Roles = "Admin,Jeweler,Manager")]
        public async Task<IActionResult> Materials()
        {
            var materials = await _materialRepo.GetAllMaterialsAsync();
            return View(materials);
        }

        // Редактирование материала (доступно только для Admin и Jeweler)
        [Authorize(Roles = "Admin,Jeweler")]
        [HttpGet]
        public async Task<IActionResult> EditMaterial(int id)
        {
            var material = await _materialRepo.GetMaterialByIdAsync(id);
            if (material == null)
                return NotFound();
            return View(material);
        }

        [Authorize(Roles = "Admin,Jeweler")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial(int id, Material material)
        {
            if (id != material.MaterialId)
                return BadRequest();
            if (await _materialRepo.UpdateMaterialAsync(material))
                return RedirectToAction("Materials");
            return View(material);
        }

        // ******************************
        // 3. Раздел "Пользователи"
        // ******************************

        // Просмотр списка пользователей (только Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return View(users);
        }

        // Удаление пользователя (только Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            if (await _userRepo.DeleteUserByIdAsync(id))
                return RedirectToAction("Users");
            return NotFound();
        }

        // Изменение роли пользователя (только Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditUserRole(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRole(int id, User user)
        {
            if (id != user.UserId)
                return BadRequest();
            if (await _userRepo.UpdateUserAsync(user))
                return RedirectToAction("Users");
            return View(user);
        }

        // ******************************
        // 4. Раздел "Отчёты"
        // ******************************

        // Страница для создания отчётов (доступна для Accountant и Admin)
        [Authorize(Roles = "Accountant,Admin")]
        public IActionResult Reports()
        {
            // Здесь можно реализовать логику генерации отчётов или передать данные в представление.
            return View();
        }
    }
}