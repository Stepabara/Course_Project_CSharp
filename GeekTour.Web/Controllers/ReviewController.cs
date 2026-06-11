using GeekTour.Web.Models.ViewModels;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeekTour.Web.Controllers;

public class ReviewController : Controller
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public IActionResult Create(int locationId)
    {
        if (!AccountController.IsAuthenticated(HttpContext))
            return RedirectToAction("Login", "Account");

        return View(new ReviewCreateViewModel { LocationId = locationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        await _reviewService.CreateAsync(model, userId);

        return RedirectToAction("Details", "Location", new { id = model.LocationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Respond(ReviewResponseViewModel model)
    {
        if (!AccountController.IsPartner(HttpContext) && !AccountController.IsAdmin(HttpContext))
            return Forbid();

        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var success = await _reviewService.RespondAsync(model.ReviewId, userId, model.Response);

        if (!success) return Forbid();
        return RedirectToAction("Details", "Location", new { id = model.ReviewId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Moderate(int reviewId, bool approve)
    {
        if (!AccountController.IsAdmin(HttpContext)) return Forbid();

        await _reviewService.ModerateAsync(reviewId, approve);
        return RedirectToAction("Pending", "Admin");
    }
}
