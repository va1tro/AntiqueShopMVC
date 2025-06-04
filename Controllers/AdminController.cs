using AntiqueShopMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;

namespace AntiqueShopMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly AntiqueShopContext _context;

        public AdminController(AntiqueShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, int? category, string sort, bool export = false)
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
                itemsQuery = itemsQuery.Where(i => i.NameItem.Contains(search));
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
                _ => itemsQuery
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
            // Возвращает форму добавления товара
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            // Возвращает форму редактирования товара
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

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Товар успешно удалён";
            return RedirectToAction("Index");
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
                itemsQuery = itemsQuery.Where(i => i.NameItem.Contains(search));

            itemsQuery = sort switch
            {
                "year_asc" => itemsQuery.OrderBy(i => i.Year),
                "year_desc" => itemsQuery.OrderByDescending(i => i.Year),
                "price_asc" => itemsQuery.OrderBy(i => i.PurchasePrice),
                "price_desc" => itemsQuery.OrderByDescending(i => i.PurchasePrice),
                "arrival_new" => itemsQuery.OrderByDescending(i => i.ArrivalDate),
                "arrival_old" => itemsQuery.OrderBy(i => i.ArrivalDate),
                _ => itemsQuery
            };

            var items = await itemsQuery.ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Название;Год;Состояние;Подлинность;Цена покупки;Цена продажи;Дата поступления");

            foreach (var item in items)
            {
                sb.AppendLine($"{item.NameItem};{item.Year};{item.Condition};{item.Authenticity};{item.PurchasePrice};{item.SellingPrice};{item.ArrivalDate?.ToString("dd.MM.yyyy")}");
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
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
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
                // Создаем новый товар
                return View(new Item());
            }
            else
            {
                // Редактируем существующий
                var item = await _context.Items.FindAsync(id);
                if (item == null) return NotFound();
                return View(item);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditItem(Item model, IFormFile imageFile)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(model.NameItem))
            {
                ModelState.AddModelError("NameItem", "Название товара обязательно");
            }

            if (model.IdCategory == null || model.IdCategory == 0)
            {
                ModelState.AddModelError("IdCategory", "Категория обязательна");
            }

            if (!ModelState.IsValid)
            {
                // Повторно загружаем списки для выпадающих списков
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
                // Добавление нового товара
                item = model;
                _context.Items.Add(item);
            }
            else
            {
                // Редактирование существующего
                item = await _context.Items.FirstOrDefaultAsync(i => i.IdItem == model.IdItem);
                if (item == null) return NotFound();

                await LogChanges(item, model);

                // Явно обновляем поля
                item.NameItem = model.NameItem;
                item.Year = model.Year;
                item.ArrivalDate = model.ArrivalDate;
                item.Condition = model.Condition;
                item.Authenticity = model.Authenticity;
                item.PurchasePrice = model.PurchasePrice;
                item.SellingPrice = model.SellingPrice;
                item.IdCategory = model.IdCategory;
                item.IdMaterial = model.IdMaterial;
                item.IdSupplier = model.IdSupplier;
                item.IdStatus = model.IdStatus;
                item.IdOriginCountry = model.IdOriginCountry;
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
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                item.Image = fileName;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Товар успешно сохранён";
            return RedirectToAction("Index");
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
                        OldValue = oldVal,
                        NewValue = newVal,
                        ChangeDate = DateTime.Now
                    });
                }
            }

            AddLog("NameItem", oldItem.NameItem, newModel.NameItem);
            AddLog("Year", oldItem.Year?.ToString(), newModel.Year?.ToString());
            AddLog("Condition", oldItem.Condition, newModel.Condition);
            AddLog("Authenticity", oldItem.Authenticity, newModel.Authenticity);
            AddLog("PurchasePrice", oldItem.PurchasePrice?.ToString(), newModel.PurchasePrice?.ToString());
            AddLog("SellingPrice", oldItem.SellingPrice?.ToString(), newModel.SellingPrice?.ToString());
            AddLog("ArrivalDate", oldItem.ArrivalDate?.ToString("yyyy-MM-dd"), newModel.ArrivalDate?.ToString("yyyy-MM-dd"));
            AddLog("Image", oldItem.Image, newModel.Image);
            AddLog("IdCategory", oldItem.IdCategory?.ToString(), newModel.IdCategory?.ToString());
            AddLog("IdMaterial", oldItem.IdMaterial?.ToString(), newModel.IdMaterial?.ToString());
            AddLog("IdSupplier", oldItem.IdSupplier?.ToString(), newModel.IdSupplier?.ToString());
            AddLog("IdStatus", oldItem.IdStatus?.ToString(), newModel.IdStatus?.ToString());
            AddLog("IdOriginCountry", oldItem.IdOriginCountry?.ToString(), newModel.IdOriginCountry?.ToString());

            if (logs.Any())
            {
                _context.ItemLogs.AddRange(logs);
            }
        }
        [HttpGet]
        public async Task<IActionResult> ItemLogs(int? id, string fieldFilter = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ItemLogs
                .Include(l => l.IdItemNavigation)
                .AsQueryable();

            // Фильтр по товару (если указан id)
            if (id.HasValue)
            {
                query = query.Where(l => l.IdItem == id.Value);
                ViewBag.ItemId = id.Value;
                ViewBag.ItemName = await _context.Items
                    .Where(i => i.IdItem == id.Value)
                    .Select(i => i.NameItem)
                    .FirstOrDefaultAsync();
            }

            // Фильтры
            if (!string.IsNullOrWhiteSpace(fieldFilter))
            {
                query = query.Where(l => l.ChangedField.Contains(fieldFilter));
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
