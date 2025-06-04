using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AntiqueShopMVC.Models;

namespace AntiqueShopMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly AntiqueShopContext _context;

        public CartController(AntiqueShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cartItems = await _context.Carts
                .Include(c => c.IdItemNavigation)
                .Where(c => c.IdUser == userId && c.IsActive == true)
                .ToListAsync();

            ViewBag.Total = cartItems.Sum(c => c.IdItemNavigation.SellingPrice.GetValueOrDefault() * c.Quantity);

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null && quantity > 0)
            {
                cart.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cartItems = await _context.Carts
                .Include(c => c.IdItemNavigation)
                .Where(c => c.IdUser == userId && c.IsActive == true)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                TempData["Message"] = "Корзина пуста";
                return RedirectToAction("Index");
            }

            foreach (var c in cartItems)
            {
                _context.Sales.Add(new Sale
                {
                    IdUser = c.IdUser,
                    IdItem = c.IdItem,
                    SaleDate = DateOnly.FromDateTime(DateTime.Now),
                    FinalPrice = c.IdItemNavigation.SellingPrice
                });
                c.IsActive = false;
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Заказ оформлен!";
            return RedirectToAction("Index");
        }
    }
}
