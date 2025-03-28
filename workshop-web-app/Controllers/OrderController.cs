using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using workshop_web_app.Models;
using workshop_web_app.Repositories;

namespace workshop_web_app.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderRepository _orderRepo;

        public OrderController(OrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepo.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // GET: /Order/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                // Сначала добавляем заказ
                int newOrderId = await _orderRepo.AddOrderAsync(order);
                
                // Если переданы детали заказа, добавляем их
                if (order.OrderDetails != null)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        detail.OrderId = newOrderId;
                        await _orderRepo.AddOrderDetailAsync(detail);
                    }
                }
                
                return RedirectToAction(nameof(Details), new { id = newOrderId });
            }
            return View(order);
        }

        // GET: /Order/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: /Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                bool updated = await _orderRepo.UpdateOrderAsync(order);
                if (updated)
                {
                    // Для обновления деталей: сначала получаем текущие детали заказа,
                    // затем удаляем их все, а после добавляем новые из переданной модели.
                    var existingOrder = await _orderRepo.GetOrderByIdAsync(order.OrderId);
                    if (existingOrder != null && existingOrder.OrderDetails != null)
                    {
                        foreach (var existingDetail in existingOrder.OrderDetails)
                        {
                            await _orderRepo.DeleteOrderDetailAsync(existingDetail.DetailsListId);
                        }
                    }

                    if (order.OrderDetails != null)
                    {
                        foreach (var detail in order.OrderDetails)
                        {
                            detail.OrderId = order.OrderId;
                            await _orderRepo.AddOrderDetailAsync(detail);
                        }
                    }

                    return RedirectToAction(nameof(Details), new { id = order.OrderId });
                }
                else
                {
                    return NotFound();
                }
            }
            return View(order);
        }

        // GET: /Order/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: /Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _orderRepo.DeleteOrderAsync(id);
            if (deleted)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound();
            }
        }
    }
}