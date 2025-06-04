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

        [HttpPost]
        public async Task<IActionResult> AddToCartAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var existing = await _context.Carts.FirstOrDefaultAsync(c =>
                c.IdUser == userId && c.IdItem == id && c.IsActive == true);

            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                _context.Carts.Add(new Cart
                {
                    IdUser = userId.Value,
                    IdItem = id,
                    Quantity = 1,
                    AddedDate = DateOnly.FromDateTime(DateTime.Now),
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Товар добавлен в корзину.";
            return RedirectToAction("Index");
            
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavoriteAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var exists = await _context.Favorites.FirstOrDefaultAsync(f => f.IdUser == userId && f.IdItem == id);
            if (exists == null)
            {
                _context.Favorites.Add(new Favorite
                {
                    IdUser = userId.Value,
                    IdItem = id
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = "Товар добавлен в избранное.";
            }
            
            else
            {
                TempData["Info"] = "Товар уже в избранном";
            }

            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Items
                .Include(i => i.IdCategoryNavigation)
                .Include(i => i.IdMaterialNavigation)
                .Include(i => i.IdSupplierNavigation)
                .Include(i => i.IdStatusNavigation)
                .Include(i => i.IdOriginCountryNavigation)
                .FirstOrDefaultAsync(i => i.IdItem == id);

            if (item == null)
                return NotFound();

            return View(item);
        }
        
        public async Task<IActionResult> UserInfo()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserInfo(User model, string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // Проверка пароля, если пользователь пытается его изменить
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (oldPassword != user.Password)
                {
                    ModelState.AddModelError("", "Неверный старый пароль");
                    return View(user);
                }

                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "Новый пароль и подтверждение не совпадают");
                    return View(user);
                }

                user.Password = newPassword;
            }

            // Обновляем остальные данные
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.MiddleName = model.MiddleName;
            user.Email = model.Email;
            user.Preferences = model.Preferences;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Данные успешно сохранены";
                return RedirectToAction("UserInfo");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                return View(user);
            }
        }
    }
}
