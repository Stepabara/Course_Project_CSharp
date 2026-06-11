using GeekTour.Web.Data;
using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.Enums;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Controllers;

public class ProfileController : Controller
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public ProfileController(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!AccountController.IsAuthenticated(HttpContext))
            return RedirectToAction("Login", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var user = await _context.Users
            .Include(u => u.OwnedLocations)
            .Include(u => u.Routes)
            .Include(u => u.Reviews)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        var model = new ProfileViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CompanyName = user.CompanyName,
            RegisteredAt = user.RegisteredAt,
            LocationCount = user.OwnedLocations.Count,
            RouteCount = user.Routes.Count,
            ReviewCount = user.Reviews.Count
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string name, string email, string? companyName)
    {
        if (!AccountController.IsAuthenticated(HttpContext))
            return RedirectToAction("Login", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // Check email uniqueness
        if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId))
        {
            ModelState.AddModelError("", "Пользователь с таким email уже существует");
            return RedirectToAction("Index");
        }

        user.Name = name;
        user.Email = email;
        if (user.Role == UserRole.Partner)
            user.CompanyName = companyName;

        await _context.SaveChangesAsync();

        // Update session
        HttpContext.Session.SetString("UserName", user.Name);

        // Go back to the page user came from, or profile if no referer
        var returnUrl = Request.Headers["Referer"].FirstOrDefault();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index");
    }
}

public class ProfileViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public UserRole Role { get; set; }
    public string? CompanyName { get; set; }
    public DateTime RegisteredAt { get; set; }
    public int LocationCount { get; set; }
    public int RouteCount { get; set; }
    public int ReviewCount { get; set; }
}
