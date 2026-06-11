using GeekTour.Web.Data;
using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.Enums;
using GeekTour.Web.Models.ViewModels;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Controllers;

[AuthorizeRole(UserRole.Admin)]
public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly IReviewService _reviewService;
    private readonly IReportService _reportService;

    public AdminController(AppDbContext context, IReviewService reviewService, IReportService reportService)
    {
        _context = context;
        _reviewService = reviewService;
        _reportService = reportService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalUsers = await _context.Users.CountAsync(),
            TotalLocations = await _context.Locations.CountAsync(),
            TotalReviews = await _context.Reviews.CountAsync(),
            TotalRoutes = await _context.Routes.CountAsync(),
            PendingReviews = await _context.Reviews.CountAsync(r => !r.IsModerated),
            VerifiedPartners = await _context.Users.CountAsync(u => u.Role == UserRole.Partner && u.IsVerified),
            RecentUsers = await _context.Users.OrderByDescending(u => u.RegisteredAt).Take(5).ToListAsync(),
            PendingReviewList = await _reviewService.GetPendingModerationAsync()
        };

        return View(model);
    }

    // Users management
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users
            .Include(u => u.OwnedLocations)
            .OrderByDescending(u => u.RegisteredAt)
            .ToListAsync();

        var model = users.Select(u => new AdminUserViewModel
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            CompanyName = u.CompanyName,
            IsVerified = u.IsVerified,
            RegisteredAt = u.RegisteredAt,
            LocationCount = u.OwnedLocations.Count
        }).ToList();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyPartner(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();
        user.IsVerified = true;
        await _context.SaveChangesAsync();
        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRole(int userId, UserRole role)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();
        user.Role = role;
        await _context.SaveChangesAsync();
        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUser(int userId, string name, string email, UserRole role, string? companyName, bool isVerified)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // Check email uniqueness (exclude current user)
        if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId))
        {
            ModelState.AddModelError("", "Пользователь с таким email уже существует");
            return RedirectToAction("Users");
        }

        user.Name = name;
        user.Email = email;
        user.Role = role;
        user.CompanyName = companyName;
        user.IsVerified = isVerified;

        await _context.SaveChangesAsync();
        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("Users");
    }

    // Locations management
    public async Task<IActionResult> Locations()
    {
        var locations = await _context.Locations
            .Include(l => l.Owner)
            .Include(l => l.Reviews)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var model = locations.Select(l => new AdminLocationViewModel
        {
            Id = l.Id,
            Name = l.Name,
            Type = l.Type,
            OwnerName = l.Owner?.Name ?? "Не назначен",
            AverageRating = l.AverageRating,
            IsActive = l.IsActive
        }).ToList();

        return View(model);
    }

    // Fandoms management
    public async Task<IActionResult> Fandoms()
    {
        var fandoms = await _context.Fandoms.OrderBy(f => f.Category).ThenBy(f => f.Name).ToListAsync();
        return View(fandoms);
    }

    [HttpPost]
    public async Task<IActionResult> AddFandom(string name, FandomCategory category)
    {
        _context.Fandoms.Add(new Fandom { Name = name, Category = category });
        await _context.SaveChangesAsync();
        return RedirectToAction("Fandoms");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFandom(int id)
    {
        var fandom = await _context.Fandoms.FindAsync(id);
        if (fandom == null) return NotFound();
        _context.Fandoms.Remove(fandom);
        await _context.SaveChangesAsync();
        return RedirectToAction("Fandoms");
    }

    // Categories management
    public async Task<IActionResult> Categories()
    {
        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        return View(categories);
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory(string name)
    {
        _context.Categories.Add(new Category { Name = name });
        await _context.SaveChangesAsync();
        return RedirectToAction("Categories");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction("Categories");
    }

    // Pending reviews moderation
    public async Task<IActionResult> Pending()
    {
        var reviews = await _reviewService.GetPendingModerationAsync();
        return View(reviews);
    }

    // Weight coefficients
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var weights = await _reportService.GetWeightCoefficientsAsync();
        return View(weights);
    }

    [HttpPost]
    public async Task<IActionResult> Settings(WeightCoefficientsViewModel model)
    {
        await _reportService.SetWeightCoefficientsAsync(model);
        return RedirectToAction("Settings");
    }
}

// Custom authorization attribute
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole _role;

    public AuthorizeRoleAttribute(UserRole role) => _role = role;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!AccountController.IsAuthenticated(context.HttpContext))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (!Enum.TryParse<UserRole>(AccountController.GetUserRole(context.HttpContext), out var userRole) || userRole != _role)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}
