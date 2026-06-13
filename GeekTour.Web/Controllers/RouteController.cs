using GeekTour.Web.Data;
using GeekTour.Web.Models.ViewModels;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Controllers;

public class RouteController : Controller
{
    private readonly IRouteService _routeService;
    private readonly ILocationService _locationService;
    private readonly Data.AppDbContext _context;

    public RouteController(IRouteService routeService, ILocationService locationService, Data.AppDbContext context)
    {
        _routeService = routeService;
        _locationService = locationService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!AccountController.IsAuthenticated(HttpContext))
            return RedirectToAction("Login", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var routes = await _routeService.GetUserRoutesAsync(userId);

        // Load public routes for the public tab
        var allPublicRoutes = await _context.Routes
            .Include(r => r.Points)
            .Where(r => r.IsPublic && r.UserId != userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var publicRouteVms = allPublicRoutes.Select(r => new RouteListViewModel
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            CreatedAt = r.CreatedAt,
            TotalDistanceKm = r.TotalDistanceKm,
            EstimatedTimeMinutes = r.EstimatedTimeMinutes,
            PointCount = r.Points.Count,
            IsPublic = r.IsPublic,
            ShareLink = r.ShareLink
        }).ToList();

        ViewBag.PublicRoutes = publicRouteVms;

        return View(routes);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var route = await _routeService.GetByIdAsync(id);
        if (route == null) return NotFound();
        return View(route);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!AccountController.IsAuthenticated(HttpContext))
            return RedirectToAction("Login", "Account");

        var filter = new LocationFilterViewModel();
        ViewBag.Locations = await _locationService.GetFilteredAsync(filter);
        return View(new RouteCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RouteCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var filter = new LocationFilterViewModel();
            ViewBag.Locations = await _locationService.GetFilteredAsync(filter);
            return View(model);
        }

        var userId = AccountController.GetUserId(HttpContext)!.Value;

        // Check if optimize checkbox is checked
        var optimize = Request.Form["optimizeRoute"].FirstOrDefault() == "true";

        if (optimize && model.LocationIds.Count >= 2)
        {
            // Create optimized route directly
            var optimizedRoute = await _routeService.OptimizeNewRoute(model.LocationIds, userId);
            if (optimizedRoute != null)
            {
                // Update name and description
                optimizedRoute.Name = model.Name;
                optimizedRoute.Description = model.Description;
                optimizedRoute.IsPublic = model.IsPublic;
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = optimizedRoute.Id });
            }
        }

        var route = await _routeService.CreateAsync(model, userId);
        return RedirectToAction("Details", new { id = route.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Optimize(int routeId)
    {
        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var optimized = await _routeService.OptimizeAsync(routeId, userId);
        if (optimized == null) return NotFound();
        return RedirectToAction("Details", new { id = optimized.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Share(int routeId)
    {
        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var link = await _routeService.GenerateShareLinkAsync(routeId, userId);
        if (link == null) return NotFound();
        return RedirectToAction("Details", new { id = routeId });
    }

    [HttpGet]
    public async Task<IActionResult> Shared(string id)
    {
        var route = await _routeService.GetByShareLinkAsync(id);
        if (route == null) return NotFound();
        return View("Details", route);
    }

    // ═══ API for Profile Tab ═══
    [HttpGet]
    public async Task<IActionResult> GetMyRoutes()
    {
        var userId = AccountController.GetUserId(HttpContext);
        if (!userId.HasValue) return Json(new List<object>());
        var routes = await _routeService.GetUserRoutesAsync(userId.Value);
        return Json(routes.Select(r => new
        {
            r.Id,
            r.Name,
            r.Description,
            r.CreatedAt,
            r.PointCount,
            r.IsPublic
        }));
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicRoutes()
    {
        var routes = await _routeService.GetPublicRoutesAsync();
        return Json(routes.Select(r => new
        {
            r.Id,
            r.Name,
            r.Description,
            r.CreatedAt,
            r.PointCount,
            r.IsPublic
        }));
    }
}
