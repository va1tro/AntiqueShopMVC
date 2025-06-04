using AntiqueShopMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AntiqueShopMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AntiqueShopContext _context;

        public AccountController(AntiqueShopContext context)
        {
            _context = context;
        }

        // Страница входа (GET)
        public IActionResult Login()
        {
            return View();
        }

        // Обработка входа (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            var user = await _context.Users
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.Login == login && u.Password == password);

            if (user != null)
            {
                // Сохраняем сессию
                HttpContext.Session.SetInt32("UserId", user.IdUser);
                HttpContext.Session.SetString("UserLogin", user.Login);
                HttpContext.Session.SetInt32("UserRole", user.IdRole);

                // Перенаправление по роли
                if (user.IdRole == 1) // Admin
                    return RedirectToAction("Index", "Admin");
                else // Client
                    return RedirectToAction("Index", "User");
            }

            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(user.Login) || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Заполните логин и пароль.";
                return View(user);
            }

            if (user.Password != confirmPassword)
            {
                ViewBag.Error = "Пароли не совпадают.";
                return View(user);
            }

            // Проверка на уникальность логина
            if (_context.Users.Any(u => u.Login == user.Login))
            {
                ViewBag.Error = "Такой логин уже существует.";
                return View(user);
            }

            user.IdRole = 2; // Роль "Client"

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Регистрация прошла успешно. Войдите под своим логином.";
            return RedirectToAction("Login");
        }
    }
}
