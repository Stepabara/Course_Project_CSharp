using GeekTour.Web.Data;
using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.Enums;
using GeekTour.Web.Models.ViewModels;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Controllers;

public class PartnerController : Controller
{
    private readonly ILocationService _locationService;
    private readonly IReviewService _reviewService;
    private readonly AppDbContext _context;

    public PartnerController(ILocationService locationService, IReviewService reviewService, AppDbContext context)
    {
        _locationService = locationService;
        _reviewService = reviewService;
        _context = context;
    }

    private async Task<bool> CheckAccess()
    {
        if (!AccountController.IsAuthenticated(HttpContext))
            return false;
        if (AccountController.IsPartner(HttpContext) || AccountController.IsAdmin(HttpContext) || AccountController.IsTourist(HttpContext))
            return true;
        return false;
    }

    // ===================== DASHBOARD =====================
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        if (!await CheckAccess()) return RedirectToAction("AccessDenied", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var locations = await _locationService.GetByOwnerAsync(userId);
        var reviews = await _reviewService.GetByOwnerLocationsAsync(userId);

        ViewBag.Locations = locations;
        ViewBag.Reviews = reviews;
        ViewBag.TotalViews = await _context.ViewStatistics.CountAsync(vs => locations.Select(l => l.Id).Contains(vs.LocationId));
        ViewBag.AvgRating = reviews.Any() ? Math.Round(reviews.Average(r => (double)r.Rating), 2) : 0.0;
        ViewBag.TotalReviews = reviews.Count;
        ViewBag.Fandoms = await _locationService.GetAllFandomsAsync();
        ViewBag.Categories = await _locationService.GetAllCategoriesAsync();

        return View();
    }

    // ===================== LOCATIONS =====================
    [HttpGet]
    public async Task<IActionResult> Locations()
    {
        if (!await CheckAccess()) return RedirectToAction("AccessDenied", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var locations = await _locationService.GetByOwnerAsync(userId);
        ViewBag.Fandoms = await _locationService.GetAllFandomsAsync();
        ViewBag.Categories = await _locationService.GetAllCategoriesAsync();

        return View(locations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLocation(LocationEditViewModel model)
    {
        if (!await CheckAccess()) return RedirectToAction("AccessDenied", "Account");

        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            ViewBag.Fandoms = await _locationService.GetAllFandomsAsync();
            ViewBag.Categories = await _locationService.GetAllCategoriesAsync();
            TempData["Error"] = "Ошибка: " + errors;
            return RedirectToAction("Locations");
        }

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        await _locationService.CreateAsync(model, userId);
        TempData["Success"] = "Заведение успешно создано!";
        return RedirectToAction("Locations");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLocation(LocationEditViewModel model)
    {
        if (!await CheckAccess()) return RedirectToAction("AccessDenied", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var isAdmin = AccountController.IsAdmin(HttpContext);
        var success = await _locationService.UpdateAsync(model, userId, isAdmin);

        if (!success) return Forbid();
        TempData["Success"] = "Заведение обновлено!";
        return RedirectToAction("Locations");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        if (!await CheckAccess()) return RedirectToAction("AccessDenied", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var isAdmin = AccountController.IsAdmin(HttpContext);
        var success = await _locationService.DeleteAsync(id, userId, isAdmin);

        if (!success) return Forbid();
        TempData["Success"] = "Заведение удалено!";
        return RedirectToAction("Locations");
    }

    // ===================== PROMOTIONS =====================
    [HttpGet]
    public async Task<IActionResult> Promotions()
    {
        if (!await CheckAccess()) return RedirectToAction("AccessDenied", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var locations = await _locationService.GetByOwnerAsync(userId);
        return View(locations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPromotion(int locationId, string title, string description, DateTime startDate, DateTime endDate)
    {
        if (!await CheckAccess()) return RedirectToAction("AccessDenied", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var location = await _context.Locations.FindAsync(locationId);
        if (location == null || location.OwnerId != userId) return Forbid();

        _context.Promotions.Add(new Promotion
        {
            LocationId = locationId,
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = "Акция добавлена!";
        return RedirectToAction("Promotions");
    }

    // ===================== REVIEWS =====================
    [HttpGet]
    public async Task<IActionResult> Reviews()
    {
        if (!await CheckAccess()) return RedirectToAction("AccessDenied", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var reviews = await _reviewService.GetByOwnerLocationsAsync(userId);
        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondToReview(int reviewId, string response)
    {
        if (!await CheckAccess()) return RedirectToAction("AccessDenied", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var success = await _reviewService.RespondAsync(reviewId, userId, response);

        if (!success) return Forbid();
        TempData["Success"] = "Ответ добавлен!";
        return RedirectToAction("Reviews");
    }
}
