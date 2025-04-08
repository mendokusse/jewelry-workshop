using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;

namespace workshop_web_app.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OrderRepository _orderRepo;
        private readonly ProductTypeRepository _productTypeRepo;
        private readonly UserRepository _userRepo;

        public OrderController(OrderRepository orderRepo, ProductTypeRepository productTypeRepo, UserRepository userRepo)
        {
            _orderRepo = orderRepo;
            _productTypeRepo = productTypeRepo;
            _userRepo = userRepo;
        }

        // GET: /Order/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var productTypes = await _productTypeRepo.GetAllProductTypesAsync();
            ViewBag.ProductTypes = new SelectList(productTypes, "ProductTypeId", "ProductTypeName");
            // Возвращаем частичное представление для создания заказа (форма в модальном окне)
            return PartialView("_CreateOrderPartial", new Order());
        }

        // POST: /Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            Console.WriteLine("В методе OrderController.Create (POST) начат процесс создания заказа.");

            // Получаем идентификатор заказчика из клеймов
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                              ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerUserId))
            {
                Console.WriteLine("Не удалось извлечь UserId из клеймов. Перенаправление на Login.");
                return RedirectToAction("Login", "Account");
            }
            Console.WriteLine("UserId из клеймов: " + userIdClaim.Value);
            Console.WriteLine("Parsed customerUserId: " + customerUserId);
            order.CustomerUserId = customerUserId;

            // Назначаем менеджера: выбираем первого пользователя с ролью "Manager"
            var managers = await _userRepo.GetUsersByRoleAsync("Manager");
            if (managers != null && managers.Count > 0)
            {
                order.ManagerUserId = managers[0].UserId;
                Console.WriteLine("Назначен менеджер с UserId: " + order.ManagerUserId);
            }
            else
            {
                order.ManagerUserId = null;
                Console.WriteLine("Менеджер не найден, ManagerUserId установлен в null.");
            }

            // Устанавливаем статус заказа "Создан" (предположим, что статус 'Создан' имеет id = 1)
            order.StatusId = 1;
            order.OrderDate = DateTime.Now;
            order.OrderUpdateDate = null;
            order.OrderDetails = null;
            Console.WriteLine("Установлены OrderStatusId = " + order.StatusId + ", OrderDate = " + order.OrderDate);

            int newOrderId = await _orderRepo.AddOrderAsync(order);
            Console.WriteLine("Получен новый идентификатор заказа: " + newOrderId);
            if (newOrderId > 0)
            {
                Console.WriteLine("Заказ успешно создан.");
                return Json(new { success = true, orderId = newOrderId });
            }
            else
            {
                ModelState.AddModelError("", "Не удалось создать заказ.");
                var productTypes = await _productTypeRepo.GetAllProductTypesAsync();
                ViewBag.ProductTypes = new SelectList(productTypes, "ProductTypeId", "ProductTypeName", order.ProductTypeId);
                Console.WriteLine("Ошибка при создании заказа.");
                return PartialView("_CreateOrderPartial", order);
            }
        }
    }
}