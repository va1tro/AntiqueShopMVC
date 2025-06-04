using AntiqueShopMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AntiqueShopMVC.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly AntiqueShopContext _context;

        public FavoritesController(AntiqueShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var favorites = await _context.Favorites
                .Include(f => f.IdItemNavigation)
                .Where(f => f.IdUser == userId)
                .ToListAsync();

            ViewBag.Count = favorites.Count;
            return View(favorites);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.IdItem == id && f.IdUser == userId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
