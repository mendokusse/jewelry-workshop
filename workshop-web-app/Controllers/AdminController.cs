using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;


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

        // Просмотр списка заказов (доступен для Admin, Jeweler, Manager)
        [Authorize(Roles = "Admin,Jeweler,Manager")]
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderRepo.GetAllOrdersAsync();
            return View("Orders/Index", orders);
        }

        // GET: Admin/EditOrder/5
        [HttpGet]
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            var statuses = await _statusRepo.GetAllStatusesAsync();
            ViewBag.Statuses = new SelectList(statuses, "StatusId", "StatusName", order.StatusId);

            return View("Orders/Edit", order);
        }

        // POST: Admin/EditOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(int id, Order postedOrder)
        {
            if (id != postedOrder.OrderId)
            {
                return BadRequest();
            }

            // Загружаем оригинальный заказ из БД
            var originalOrder = await _orderRepo.GetOrderByIdAsync(id);
            if (originalOrder == null)
            {
                return NotFound();
            }

            // Если пользователь - менеджер (или администратор), обновляем общую информацию заказа.
            // Менеджеры могут редактировать все поля заказа.
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                // Обновляем поля, которые редактируются в форме.
                originalOrder.ProductTypeId = postedOrder.ProductTypeId;
                originalOrder.OrderComment = postedOrder.OrderComment;
                originalOrder.OrderPrice = postedOrder.OrderPrice;
                originalOrder.OrderDate = postedOrder.OrderDate;
                originalOrder.OrderUpdateDate = postedOrder.OrderUpdateDate;
                // Дополнительные поля, если форма их предоставляет.
            }

            // Если пользователь - ювелир (и не менеджер), то он может редактировать только материалы заказа.
            if (User.IsInRole("Jeweler") && !User.IsInRole("Manager"))
            {
                // Здесь мы оставляем оригинальные данные заказа, за исключением деталей заказа.
                // Обновляем только список OrderDetails.
                originalOrder.OrderDetails = postedOrder.OrderDetails;
            }
            else
            {
                // Для пользователей с полными правами (Admin/Manager) мы можем также обновлять детали заказа.
                // В этом примере реализуем полное обновление деталей: удаляем старые и добавляем новые.
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

            // Теперь обновляем заказ в БД
            bool updated = await _orderRepo.UpdateOrderAsync(originalOrder);
            if (!updated)
            {
                ModelState.AddModelError("", "Не удалось обновить заказ.");
                return View(originalOrder);
            }

            // Обновляем детали заказа (сохранение деталей происходит отдельно, если их изменили)
            if (originalOrder.OrderDetails != null)
            {
                foreach (var detail in originalOrder.OrderDetails)
                {
                    // Обязательно устанавливаем OrderId для каждой детали
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
