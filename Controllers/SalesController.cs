using AntiqueShopMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AntiqueShopMVC.Controllers
{
    public class SalesController : Controller
    {
        private readonly AntiqueShopContext _context;

        public SalesController(AntiqueShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var orders = await _context.Sales
                .Include(s => s.IdItemNavigation)
                .Where(s => s.IdUser == userId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            ViewBag.OrdersCount = orders.Count;
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var order = await _context.Sales
                .FirstOrDefaultAsync(s => s.IdSale == id && s.IdUser == userId);

            if (order != null)
            {
                _context.Sales.Remove(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Заказ успешно отменён";
            }
            else
            {
                TempData["Error"] = "Не удалось найти указанный заказ";
            }

            return RedirectToAction("Index");
        }
    }
}
