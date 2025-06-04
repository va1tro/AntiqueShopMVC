using AntiqueShopMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AntiqueShopMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly AntiqueShopContext _context;

        public UserController(AntiqueShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string category, string sort)
        {
            var itemsQuery = _context.Items
                .Include(i => i.IdCategoryNavigation)
                .Include(i => i.IdMaterialNavigation)
                .Include(i => i.IdSupplierNavigation)
                .Include(i => i.IdStatusNavigation)
                .Include(i => i.IdOriginCountryNavigation)
                .AsQueryable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(search))
            {
                itemsQuery = itemsQuery.Where(i => i.NameItem.Contains(search));
            }

            // Фильтрация
            if (!string.IsNullOrWhiteSpace(category) && category != "Все")
            {
                itemsQuery = itemsQuery.Where(i => i.IdCategoryNavigation.NameCategory == category);
            }

            // Сортировка
            itemsQuery = sort switch
            {
                "year_asc" => itemsQuery.OrderBy(i => i.Year),
                "year_desc" => itemsQuery.OrderByDescending(i => i.Year),
                "price_asc" => itemsQuery.OrderBy(i => i.SellingPrice),
                "price_desc" => itemsQuery.OrderByDescending(i => i.SellingPrice),
                "arrival_new" => itemsQuery.OrderByDescending(i => i.ArrivalDate),
                "arrival_old" => itemsQuery.OrderBy(i => i.ArrivalDate),
                _ => itemsQuery
            };

            ViewBag.Categories = await _context.Categories.Select(c => c.NameCategory).ToListAsync();
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sort;

            return View(await itemsQuery.ToListAsync());
        }
    }
}
