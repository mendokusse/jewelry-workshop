using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

//подтянуть тип изделия в редактирование

namespace workshop_web_app.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly OrderRepository _orderRepo;
        private readonly MaterialRepository _materialRepo;
        private readonly UserRepository _userRepo;
        private readonly StatusRepository _statusRepo;

        public AdminController(OrderRepository orderRepo, MaterialRepository materialRepo, UserRepository userRepo, StatusRepository statusRepo)
        {
            _orderRepo = orderRepo;
            _materialRepo = materialRepo;
            _userRepo = userRepo;
            _statusRepo = statusRepo;
        }

        [Authorize(Roles = "Admin,Jeweler,Manager,Accountant")]
        public async Task<IActionResult> Index() {
            return View();
        }

        // ******************************
        // 1. Раздел "Заказы"
        // ******************************
        [Authorize(Roles = "Admin,Jeweler,Manager")]
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderRepo.GetAllOrdersAsync();
                    
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                int currentUserId = int.Parse(userIdClaim.Value);
                if (User.IsInRole("Manager"))
                {
                    orders = orders.Where(o => o.ManagerUserId == currentUserId).ToList();
                }
                else if (User.IsInRole("Jeweler"))
                {
                    orders = orders.Where(o => o.JewelerUserId == currentUserId).ToList();
                }
            }
            return View("Orders/Index", orders);
        }

        // GET: Admin/EditOrder/5
        [HttpGet]
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            var jewelers = await _userRepo.GetUsersByRoleAsync("Jeweler");

            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (User.IsInRole("Manager") && !string.IsNullOrEmpty(currentUserEmail))
            {
                jewelers = jewelers.FindAll(u => !u.UserEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase));
            }
            ViewBag.Jewelers = new SelectList(jewelers, "UserId", "UserName", order.JewelerUserId);

            var statuses = await _statusRepo.GetAllStatusesAsync();
            ViewBag.Statuses = new SelectList(statuses, "StatusId", "StatusName", order.StatusId);

            return View("Orders/Edit", order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(int id, Order postedOrder)
        {
            if (id != postedOrder.OrderId)
            {
                return BadRequest();
            }

            var originalOrder = await _orderRepo.GetOrderByIdAsync(id);
            if (originalOrder == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                originalOrder.ProductTypeId = postedOrder.ProductTypeId;
                originalOrder.OrderComment = postedOrder.OrderComment;
                originalOrder.OrderPrice = postedOrder.OrderPrice;
                originalOrder.OrderDate = postedOrder.OrderDate;
                originalOrder.OrderUpdateDate = postedOrder.OrderUpdateDate;
                originalOrder.JewelerUserId = postedOrder.JewelerUserId;
            }

            if (User.IsInRole("Jeweler") && !User.IsInRole("Manager"))
            {
                originalOrder.OrderDetails = postedOrder.OrderDetails;
            }
            else
            {
                var currentOrder = await _orderRepo.GetOrderByIdAsync(originalOrder.OrderId);
                if (currentOrder != null && currentOrder.OrderDetails != null)
                {
                    foreach (var detail in currentOrder.OrderDetails)
                    {
                        await _orderRepo.DeleteOrderDetailAsync(detail.DetailsListId);
                    }
                }
                if (postedOrder.OrderDetails != null)
                {
                    originalOrder.OrderDetails = postedOrder.OrderDetails;
                }
            }

            bool updated = await _orderRepo.UpdateOrderAsync(originalOrder);
            if (!updated)
            {
                ModelState.AddModelError("", "Не удалось обновить заказ.");
                return View(originalOrder);
            }

            if (originalOrder.OrderDetails != null)
            {
                foreach (var detail in originalOrder.OrderDetails)
                {
                    detail.OrderId = originalOrder.OrderId;
                    await _orderRepo.AddOrderDetailAsync(detail);
                }
            }

            return RedirectToAction("Orders", "Admin");
        }

        [Authorize(Roles = "Admin,Jeweler,Manager")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View("Orders/Details", order);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrderConfirmed(int id)
        {
            bool deleted = await _orderRepo.DeleteOrderAsync(id);
            if (deleted)
            {
                return RedirectToAction("Orders", "Admin");
            }
            return NotFound();
        }

        // ******************************
        // 2. Раздел "Материалы"
        // ******************************

        // Просмотр списка материалов (доступен для Admin, Jeweler, Manager)
        [Authorize(Roles = "Admin,Jeweler,Manager")]
        public async Task<IActionResult> Materials()
        {
            var materials = await _materialRepo.GetAllMaterialsAsync();
            return View("Materials/Index", materials);
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
