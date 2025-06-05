using AntiqueShopMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;
using System.IO;

namespace AntiqueShopMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly AntiqueShopContext _context;

        public AdminController(AntiqueShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, int? category, string sort)
        {
            var itemsQuery = _context.Items
                .Include(i => i.IdCategoryNavigation)
                .AsQueryable();

            // Фильтрация по категории
            if (category.HasValue && category.Value > 0)
            {
                itemsQuery = itemsQuery.Where(i => i.IdCategory == category.Value);
            }

            // Поиск
            if (!string.IsNullOrWhiteSpace(search))
            {
                itemsQuery = itemsQuery.Where(i => i.NameItem.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Сортировка
            itemsQuery = sort switch
            {
                "year_asc" => itemsQuery.OrderBy(i => i.Year),
                "year_desc" => itemsQuery.OrderByDescending(i => i.Year),
                "price_asc" => itemsQuery.OrderBy(i => i.PurchasePrice),
                "price_desc" => itemsQuery.OrderByDescending(i => i.PurchasePrice),
                "arrival_new" => itemsQuery.OrderByDescending(i => i.ArrivalDate),
                "arrival_old" => itemsQuery.OrderBy(i => i.ArrivalDate),
                _ => itemsQuery.OrderBy(i => i.IdItem)
            };

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CurrentCategory = category?.ToString();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sort;

            return View(await itemsQuery.ToListAsync());
        }

        [HttpGet]
        public IActionResult AddItem()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            try
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Товар успешно удалён";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении товара: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ExportToCsv(string search, int? category, string sort)
        {
            var itemsQuery = _context.Items
                .Include(i => i.IdCategoryNavigation)
                .AsQueryable();

            // Фильтрация
            if (category.HasValue && category.Value > 0)
                itemsQuery = itemsQuery.Where(i => i.IdCategory == category.Value);

            if (!string.IsNullOrWhiteSpace(search))
                itemsQuery = itemsQuery.Where(i => i.NameItem.Contains(search, StringComparison.OrdinalIgnoreCase));

            itemsQuery = sort switch
            {
                "year_asc" => itemsQuery.OrderBy(i => i.Year),
                "year_desc" => itemsQuery.OrderByDescending(i => i.Year),
                "price_asc" => itemsQuery.OrderBy(i => i.PurchasePrice),
                "price_desc" => itemsQuery.OrderByDescending(i => i.PurchasePrice),
                "arrival_new" => itemsQuery.OrderByDescending(i => i.ArrivalDate),
                "arrival_old" => itemsQuery.OrderBy(i => i.ArrivalDate),
                _ => itemsQuery.OrderBy(i => i.IdItem)
            };

            var items = await itemsQuery.ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Название;Год;Состояние;Подлинность;Цена покупки;Цена продажи;Дата поступления;Категория");

            foreach (var item in items)
            {
                var categoryName = item.IdCategoryNavigation?.NameCategory ?? "N/A";
                sb.AppendLine($"{item.NameItem};{item.Year};{item.Condition};{item.Authenticity};{item.PurchasePrice};{item.SellingPrice};{item.ArrivalDate?.ToString("dd.MM.yyyy")};{categoryName}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", "Список_товаров.csv");
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
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.InnerException?.Message ?? ex.Message}");
                return View(user);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddEditItem(int? id)
        {
            // Загружаем связанные данные для выпадающих списков
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "IdCategory", "NameCategory");
            ViewBag.Materials = new SelectList(await _context.Materials.ToListAsync(), "IdMaterial", "NameMaterial");
            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "IdSupplier", "NameSupplier");
            ViewBag.Statuses = new SelectList(await _context.Statuses.ToListAsync(), "IdStatus", "NameStatus");
            ViewBag.OriginCountries = new SelectList(await _context.OriginCountries.ToListAsync(), "IdOriginCountry", "NameCountry");

            if (id == null || id == 0)
            {
                return View(new Item());
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditItem(Item model, IFormFile imageFile)
        {
            // Логирование для диагностики
            System.Diagnostics.Debug.WriteLine($"AddEditItem called: IdItem={model.IdItem}, NameItem={model.NameItem}, IdCategory={model.IdCategory}");

            // Проверка внешних ключей
            if (model.IdCategory.HasValue && !await _context.Categories.AnyAsync(c => c.IdCategory == model.IdCategory))
            {
                ModelState.AddModelError("IdCategory", "Выбранная категория не существует");
            }
            if (model.IdMaterial.HasValue && !await _context.Materials.AnyAsync(m => m.IdMaterial == model.IdMaterial))
            {
                ModelState.AddModelError("IdMaterial", "Выбранный материал не существует");
            }
            if (model.IdSupplier.HasValue && !await _context.Suppliers.AnyAsync(s => s.IdSupplier == model.IdSupplier))
            {
                ModelState.AddModelError("IdSupplier", "Выбранный поставщик не существует");
            }
            if (model.IdStatus.HasValue && !await _context.Statuses.AnyAsync(s => s.IdStatus == model.IdStatus))
            {
                ModelState.AddModelError("IdStatus", "Выбранный статус не существует");
            }
            if (model.IdOriginCountry.HasValue && !await _context.OriginCountries.AnyAsync(o => o.IdOriginCountry == model.IdOriginCountry))
            {
                ModelState.AddModelError("IdOriginCountry", "Выбранная страна не существует");
            }

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState is invalid:");
                foreach (var error in ModelState)
                {
                    System.Diagnostics.Debug.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "IdCategory", "NameCategory");
                ViewBag.Materials = new SelectList(await _context.Materials.ToListAsync(), "IdMaterial", "NameMaterial");
                ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "IdSupplier", "NameSupplier");
                ViewBag.Statuses = new SelectList(await _context.Statuses.ToListAsync(), "IdStatus", "NameStatus");
                ViewBag.OriginCountries = new SelectList(await _context.OriginCountries.ToListAsync(), "IdOriginCountry", "NameCountry");
                return View(model);
            }

            Item item;
            if (model.IdItem == 0)
            {
                System.Diagnostics.Debug.WriteLine("Adding new item");
                item = model;
                _context.Items.Add(item);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Editing item with Id: {model.IdItem}");
                item = await _context.Items.FirstOrDefaultAsync(i => i.IdItem == model.IdItem);
                if (item == null)
                {
                    System.Diagnostics.Debug.WriteLine("Item not found");
                    return NotFound();
                }
                await LogChanges(item, model);
            }

            // Обновляем поля
            item.NameItem = model.NameItem;
            item.Year = model.Year;
            item.Condition = model.Condition;
            item.Authenticity = model.Authenticity;
            item.PurchasePrice = model.PurchasePrice;
            item.SellingPrice = model.SellingPrice;
            item.ArrivalDate = model.ArrivalDate;
            item.IdCategory = model.IdCategory;
            item.IdMaterial = model.IdMaterial;
            item.IdSupplier = model.IdSupplier;
            item.IdStatus = model.IdStatus;
            item.IdOriginCountry = model.IdOriginCountry;

            // Обработка изображения
            if (imageFile != null && imageFile.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Uploading image: {imageFile.FileName}");
                if (imageFile.Length > 5 * 1024 * 1024) // Ограничение 5 МБ
                {
                    ModelState.AddModelError("imageFile", "Размер файла не должен превышать 5 МБ");
                    ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "IdCategory", "NameCategory");
                    ViewBag.Materials = new SelectList(await _context.Materials.ToListAsync(), "IdMaterial", "NameMaterial");
                    ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "IdSupplier", "NameSupplier");
                    ViewBag.Statuses = new SelectList(await _context.Statuses.ToListAsync(), "IdStatus", "NameStatus");
                    ViewBag.OriginCountries = new SelectList(await _context.OriginCountries.ToListAsync(), "IdOriginCountry", "NameCountry");
                    return View(model);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    item.Image = fileName;
                }
                catch (IOException ex)
                {
                    ModelState.AddModelError("imageFile", $"Ошибка при загрузке изображения: {ex.Message}");
                    ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "IdCategory", "NameCategory");
                    ViewBag.Materials = new SelectList(await _context.Materials.ToListAsync(), "IdMaterial", "NameMaterial");
                    ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "IdSupplier", "NameSupplier");
                    ViewBag.Statuses = new SelectList(await _context.Statuses.ToListAsync(), "IdStatus", "NameStatus");
                    ViewBag.OriginCountries = new SelectList(await _context.OriginCountries.ToListAsync(), "IdOriginCountry", "NameCountry");
                    return View(model);
                }
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("Saving changes to database");
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Товар успешно сохранён";
                System.Diagnostics.Debug.WriteLine("Redirecting to Index");
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error: {ex.InnerException?.Message ?? ex.Message}");
                ModelState.AddModelError("", $"Ошибка при сохранении данных: {ex.InnerException?.Message ?? ex.Message}");
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "IdCategory", "NameCategory");
                ViewBag.Materials = new SelectList(await _context.Materials.ToListAsync(), "IdMaterial", "NameMaterial");
                ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "IdSupplier", "NameSupplier");
                ViewBag.Statuses = new SelectList(await _context.Statuses.ToListAsync(), "IdStatus", "NameStatus");
                ViewBag.OriginCountries = new SelectList(await _context.OriginCountries.ToListAsync(), "IdOriginCountry", "NameCountry");
                return View(model);
            }
        }

        private async Task LogChanges(Item oldItem, Item newModel)
        {
            var logs = new List<ItemLog>();

            void AddLog(string field, string oldVal, string newVal)
            {
                if (oldVal != newVal)
                {
                    logs.Add(new ItemLog
                    {
                        IdItem = oldItem.IdItem,
                        ChangedField = field,
                        OldValue = oldVal ?? "null",
                        NewValue = newVal ?? "null",
                        ChangeDate = DateTime.Now
                    });
                }
            }

            AddLog("NameItem", oldItem.NameItem, newModel.NameItem);
            AddLog("Year", oldItem.Year?.ToString(), newModel.Year?.ToString());
            AddLog("Condition", oldItem.Condition, newModel.Condition);
            AddLog("Authenticity", oldItem.Authenticity, newModel.Authenticity);
            AddLog("PurchasePrice", oldItem.PurchasePrice?.ToString("F2"), newModel.PurchasePrice?.ToString("F2"));
            AddLog("SellingPrice", oldItem.SellingPrice?.ToString("F2"), newModel.SellingPrice?.ToString("F2"));
            AddLog("ArrivalDate", oldItem.ArrivalDate?.ToString("yyyy-MM-dd"), newModel.ArrivalDate?.ToString("yyyy-MM-dd"));
            AddLog("Image", oldItem.Image, newModel.Image);
            AddLog("IdCategory", oldItem.IdCategory?.ToString(), newModel.IdCategory?.ToString());
            AddLog("IdMaterial", oldItem.IdMaterial?.ToString(), newModel.IdMaterial?.ToString());
            AddLog("IdSupplier", oldItem.IdSupplier?.ToString(), newModel.IdSupplier?.ToString());
            AddLog("IdStatus", oldItem.IdStatus?.ToString(), newModel.IdStatus?.ToString());
            AddLog("IdOriginCountry", oldItem.IdOriginCountry?.ToString(), newModel.IdOriginCountry?.ToString());

            if (logs.Any())
            {
                try
                {
                    _context.ItemLogs.AddRange(logs);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"LogChanges error: {ex.InnerException?.Message ?? ex.Message}");
                    // Логирование не должно прерывать основной процесс
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> ItemLogs(int? id, string fieldFilter = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ItemLogs
                .Include(l => l.IdItemNavigation)
                .AsQueryable();

            if (id.HasValue)
            {
                query = query.Where(l => l.IdItem == id.Value);
                ViewBag.ItemId = id.Value;
                ViewBag.ItemName = await _context.Items
                    .Where(i => i.IdItem == id.Value)
                    .Select(i => i.NameItem)
                    .FirstOrDefaultAsync();
            }

            if (!string.IsNullOrWhiteSpace(fieldFilter))
            {
                query = query.Where(l => l.ChangedField.Contains(fieldFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (startDate.HasValue)
            {
                query = query.Where(l => l.ChangeDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(l => l.ChangeDate < endDate.Value.AddDays(1));
            }

            query = query.OrderByDescending(l => l.ChangeDate);

            return View(await query.ToListAsync());
        }
    }
}