using GeekTour.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Controllers;

public class GuestController : Controller
{
    private readonly AppDbContext _context;

    public GuestController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // Redirect to home if authenticated
        if (AccountController.IsAuthenticated(HttpContext))
            return RedirectToAction("Index", "Home");

        var locations = _context.Locations
            .Include(l => l.Reviews)
            .Include(l => l.LocationFandoms)
            .ThenInclude(lf => lf.Fandom)
            .Where(l => l.IsActive)
            .Select(l => new GeekTour.Web.Models.ViewModels.LocationListViewModel
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Address = l.Address,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                AverageRating = l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : 0,
                Type = l.Type,
                FandomNames = l.LocationFandoms.Select(lf => lf.Fandom.Name).ToList(),
                WorkingHoursJson = l.WorkingHoursJson
            }).ToList();

        ViewBag.Fandoms = _context.Fandoms.ToList();
        ViewBag.Categories = _context.Categories.ToList();

        return View(locations);
    }
}
