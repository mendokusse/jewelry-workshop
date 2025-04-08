using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System;
using System.Linq;
using System.Text;

// вывод материалов в список
// подсчёт себестоимости заказа 

namespace workshop_web_app.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly OrderRepository _orderRepo;
        private readonly MaterialRepository _materialRepo;
        private readonly UserRepository _userRepo;
        private readonly StatusRepository _statusRepo;
        private readonly RoleRepository _roleRepo;
        private readonly ProductTypeRepository _productTypeRepo;

        public AdminController(OrderRepository orderRepo, MaterialRepository materialRepo, UserRepository userRepo, StatusRepository statusRepo, RoleRepository roleRepo, ProductTypeRepository productTypeRepo)
        {
            _orderRepo = orderRepo;
            _materialRepo = materialRepo;
            _userRepo = userRepo;
            _statusRepo = statusRepo;
            _roleRepo = roleRepo;
            _productTypeRepo = productTypeRepo;
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

            var productTypes = await _productTypeRepo.GetAllProductTypesAsync();
            ViewBag.ProductTypes = new SelectList(productTypes, "ProductTypeId", "ProductTypeName", order.ProductTypeId);

            var materials = await _materialRepo.GetAllMaterialsAsync();
            ViewBag.Materials = new SelectList(materials, "MaterialId", "MaterialName");

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
            return View("Materials/Edit", material);
        }

        [Authorize(Roles = "Admin,Jeweler")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial(int id, Material postedMaterial)
        {
            if (id != postedMaterial.MaterialId)
                return BadRequest();

            // Загружаем оригинальный материал из базы
            var originalMaterial = await _materialRepo.GetMaterialByIdAsync(id);
            if (originalMaterial == null)
                return NotFound();

            originalMaterial.MaterialName = postedMaterial.MaterialName;
            originalMaterial.MaterialPrice = postedMaterial.MaterialPrice;
            originalMaterial.MaterialQuantity = postedMaterial.MaterialQuantity;

            bool updated = await _materialRepo.UpdateMaterialAsync(originalMaterial);
            if (updated)
                return RedirectToAction("Materials");
            return View(originalMaterial);
        }

        // ******************************
        // 3. Раздел "Пользователи"
        // ******************************

        public async Task<IActionResult> Users()
        {
            var users = await _userRepo.GetAllUsersAsync();
            var roles = await _roleRepo.GetAllRolesAsync();
            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName");
            return View("Users/Index", users);
        }

        // POST: /Admin/UpdateUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(int UserId, int RoleId)
        {
            var user = await _userRepo.GetUserByIdAsync(UserId);
            if (user == null)
                return NotFound();

            user.RoleId = RoleId;
            bool updated = await _userRepo.UpdateUserAsync(user);
            if (updated)
                return RedirectToAction("Users");
            
            ModelState.AddModelError("", "Ошибка при обновлении роли пользователя.");
            var users = await _userRepo.GetAllUsersAsync();
            var roles = await _roleRepo.GetAllRolesAsync();
            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName");
            return View("Users/Index", users);
        }

        // ******************************
        // 4. Раздел "Отчёты"
        // ******************************

        
        // Страница для создания отчётов (доступна для Accountant и Admin)
        [Authorize(Roles = "Admin,Accountant")]
        public IActionResult Reports()
        {
            return View("Reports/Index");
        }

        [Authorize(Roles = "Admin,Accountant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(DateTime? startDate, DateTime? endDate)
        {
            var orders = await _orderRepo.GetAllOrdersAsync();

            if (startDate.HasValue && endDate.HasValue)
            {
                orders = orders
                    .Where(o => o.OrderDate.HasValue &&
                                o.OrderDate.Value >= startDate.Value &&
                                o.OrderDate.Value <= endDate.Value)
                    .ToList();
            }

            var completedOrders = orders
                .Where(o => o.Status != null && o.Status.StatusName.Equals("Выполнен", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var managerReport = completedOrders
                .Where(o => o.ManagerUser != null)
                .GroupBy(o => new { o.ManagerUser.UserId, o.ManagerUser.UserName })
                .Select(g => new {
                    WorkerId = g.Key.UserId,
                    WorkerName = g.Key.UserName,
                    OrderCount = g.Count(),
                    TotalSum = g.Sum(o => o.OrderPrice ?? 0)
                })
                .ToList();

            var jewelerReport = completedOrders
                .Where(o => o.JewelerUser != null)
                .GroupBy(o => new { o.JewelerUser.UserId, o.JewelerUser.UserName })
                .Select(g => new {
                    WorkerId = g.Key.UserId,
                    WorkerName = g.Key.UserName,
                    OrderCount = g.Count(),
                    TotalSum = g.Sum(o => o.OrderPrice ?? 0)
                })
                .ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Отчет о выполненных заказах");
            csv.AppendLine($"Дата отчета: {DateTime.Now}");
            if (startDate.HasValue && endDate.HasValue)
            {
                csv.AppendLine($"За период: {startDate.Value:dd.MM.yyyy} - {endDate.Value:dd.MM.yyyy}");
            }
            csv.AppendLine();

            csv.AppendLine("Менеджеры");
            csv.AppendLine("Имя работника,Выполненные заказы,Итого");
            foreach (var item in managerReport)
            {
                csv.AppendLine($"{item.WorkerName},{item.OrderCount},{item.TotalSum.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}");
            }
            csv.AppendLine();

            csv.AppendLine("Ювелиры");
            csv.AppendLine("Имя работника,Выполненные заказы,Итого");
            foreach (var item in jewelerReport)
            {
                csv.AppendLine($"{item.WorkerName},{item.OrderCount},{item.TotalSum.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}");
            }

            byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "report.csv");
        }
    }
}
