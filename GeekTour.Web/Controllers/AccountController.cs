using GeekTour.Web.Models.ViewModels;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeekTour.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Жёсткая проверка админа — два ифа на почту и пароль
        if (model.Email == "adm@gmail.com" && model.Password == "123123")
        {
            // Get the real admin user from DB (admin@geektour.ru, users[0])
            var admin = await _authService.GetUserByEmailAsync("admin@geektour.ru");
            HttpContext.Session.SetInt32("UserId", admin!.Id);
            HttpContext.Session.SetString("UserName", admin.Name);
            HttpContext.Session.SetString("UserRole", admin.Role.ToString());
            return RedirectToAction("Index", "Home");
        }

        var (success, message, user) = await _authService.LoginAsync(model);
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        HttpContext.Session.SetInt32("UserId", user!.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, message, user) = await _authService.RegisterAsync(model);
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        HttpContext.Session.SetInt32("UserId", user!.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    public static int? GetUserId(HttpContext context) => context.Session.GetInt32("UserId");
    public static string? GetUserRole(HttpContext context) => context.Session.GetString("UserRole");
    public static bool IsAuthenticated(HttpContext context) => context.Session.GetInt32("UserId").HasValue;
    public static bool IsAdmin(HttpContext context) => GetUserRole(context) == "Admin";
    public static bool IsPartner(HttpContext context) => GetUserRole(context) == "Partner";
    public static bool IsTourist(HttpContext context) => GetUserRole(context) == "Tourist";
}
