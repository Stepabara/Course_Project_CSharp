using GeekTour.Web.Models.ViewModels;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeekTour.Web.Controllers;

public class LocationController : Controller
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(LocationFilterViewModel filter)
    {
        var locations = await _locationService.GetFilteredAsync(filter);
        ViewBag.Fandoms = await _locationService.GetAllFandomsAsync();
        ViewBag.Categories = await _locationService.GetAllCategoriesAsync();
        return View(locations);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = AccountController.GetUserId(HttpContext);
        var location = await _locationService.GetByIdAsync(id, userId);
        if (location == null) return NotFound();
        return View(location);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!AccountController.IsAuthenticated(HttpContext))
            return RedirectToAction("Login", "Account");
        if (!AccountController.IsPartner(HttpContext) && !AccountController.IsAdmin(HttpContext))
            return RedirectToAction("AccessDenied", "Account");

        ViewBag.Fandoms = await _locationService.GetAllFandomsAsync();
        ViewBag.Categories = await _locationService.GetAllCategoriesAsync();
        return View(new LocationEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Fandoms = await _locationService.GetAllFandomsAsync();
            ViewBag.Categories = await _locationService.GetAllCategoriesAsync();
            return View(model);
        }

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        await _locationService.CreateAsync(model, userId);

        var returnUrl = Request.Form["returnUrl"].FirstOrDefault();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!AccountController.IsAuthenticated(HttpContext))
            return RedirectToAction("Login", "Account");

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var location = await _locationService.GetByIdAsync(id);
        if (location == null) return NotFound();

        var editModel = new LocationEditViewModel
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Type = location.Type,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            WorkingHoursJson = location.WorkingHoursJson,
            CategoryId = location.CategoryId,
            FandomIds = location.Fandoms.Select(f => f.Id).ToList(),
            IsActive = true
        };

        ViewBag.Fandoms = await _locationService.GetAllFandomsAsync();
        ViewBag.Categories = await _locationService.GetAllCategoriesAsync();
        return View(editModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LocationEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Fandoms = await _locationService.GetAllFandomsAsync();
            ViewBag.Categories = await _locationService.GetAllCategoriesAsync();
            return View(model);
        }

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var isAdmin = AccountController.IsAdmin(HttpContext);
        var success = await _locationService.UpdateAsync(model, userId, isAdmin);

        if (!success) return Forbid();

        // Redirect back to the view that requested the edit
        var returnUrl = Request.Form["returnUrl"].FirstOrDefault();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Details", new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var isAdmin = AccountController.IsAdmin(HttpContext);
        var success = await _locationService.DeleteAsync(id, userId, isAdmin);

        if (!success) return Forbid();

        // If admin came from admin panel, go back there
        var returnUrl = Request.Form["returnUrl"].FirstOrDefault();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index");
    }
}
