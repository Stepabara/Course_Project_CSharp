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

    // ═══ API for Profile Modal ═══
    [HttpGet]
    public async Task<IActionResult> GetProfileData()
    {
        var userId = AccountController.GetUserId(HttpContext);
        if (!userId.HasValue) return Json(new { error = "not_authenticated" });

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null) return Json(new { error = "not_found" });

        return Json(new
        {
            user.Id,
            user.Name,
            user.Email,
            Role = user.Role.ToString(),
            user.CompanyName,
            user.AvatarPath,
            user.RegisteredAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Update(string name, string email, string? companyName, string? password)
    {
        if (!AccountController.IsAuthenticated(HttpContext))
            return Unauthorized();

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // Check email uniqueness
        if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId))
        {
            return BadRequest("Пользователь с таким email уже существует");
        }

        user.Name = name;
        user.Email = email;
        if (user.Role == UserRole.Partner)
            user.CompanyName = companyName;

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(password) && password.Length >= 6)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "GeekTourSalt2024"));
            user.PasswordHash = Convert.ToBase64String(bytes);
        }

        await _context.SaveChangesAsync();

        // Update session
        HttpContext.Session.SetString("UserName", user.Name);

        return Ok();
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
