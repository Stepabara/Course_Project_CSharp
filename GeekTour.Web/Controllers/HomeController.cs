using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.Enums;
using GeekTour.Web.Models.ViewModels;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeekTour.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILocationService _locationService;

    public HomeController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    public async Task<IActionResult> Index()
    {
        var filter = new LocationFilterViewModel();
        var locations = await _locationService.GetFilteredAsync(filter);
        var fandoms = await _locationService.GetAllFandomsAsync();
        var categories = await _locationService.GetAllCategoriesAsync();
        ViewBag.Fandoms = fandoms;
        ViewBag.Categories = categories;

        var userId = AccountController.GetUserId(HttpContext);
        if (userId.HasValue)
        {
            ViewBag.Recommended = await _locationService.GetRecommendedAsync(userId.Value);
        }

        return View(locations);
    }

    [HttpGet]
    public async Task<IActionResult> GetFilteredLocations(
        string? searchQuery = null,
        int? type = null,
        [FromQuery] List<int>? fandomId = null,
        int? categoryId = null,
        double? minRating = null,
        string sortBy = "rating")
    {
        var filter = new LocationFilterViewModel
        {
            SearchQuery = searchQuery,
            Type = type.HasValue ? (LocationType?)type.Value : null,
            FandomIds = fandomId ?? new List<int>(),
            CategoryId = categoryId,
            MinRating = minRating,
            SortBy = sortBy
        };

        var locations = await _locationService.GetFilteredAsync(filter);

        return Json(locations.Select(l => new
        {
            l.Id,
            l.Name,
            l.Description,
            l.Address,
            l.Latitude,
            l.Longitude,
            l.AverageRating,
            l.Type,
            l.ReviewCount,
            l.CategoryName,
            l.FandomIds,
            l.FandomNames,
            l.WorkingHoursJson,
            l.MainImagePath
        }));
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
