using GeekTour.Web.Data;
using GeekTour.Web.Models.ViewModels;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeekTour.Web.Controllers;

public class ReviewController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public ReviewController(IReviewService reviewService, AppDbContext context, IFileStorageService fileStorage)
    {
        _reviewService = reviewService;
        _context = context;
        _fileStorage = fileStorage;
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

    // ═══ AJAX Create Review (from modal) ═══
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAjax()
    {
        if (!AccountController.IsAuthenticated(HttpContext)) return Unauthorized();
        var userId = AccountController.GetUserId(HttpContext)!.Value;
        var locationId = int.Parse(Request.Form["locationId"]);
        var rating = int.Parse(Request.Form["rating"]);
        var text = Request.Form["text"];
        var review = await _reviewService.CreateAsync(new ReviewCreateViewModel { LocationId = locationId, Rating = rating, Text = text }, userId);
        var photoPaths = new List<string>();
        foreach (var key in Request.Form.Keys)
        {
            if (key.StartsWith("PhotoBase64_"))
            {
                var base64 = Request.Form[key].ToString();
                if (!string.IsNullOrEmpty(base64))
                {
                    var path = await _fileStorage.SaveBase64ImageAsync(base64, "reviews");
                    if (path != null) photoPaths.Add(path);
                }
            }
        }
        if (photoPaths.Count > 0)
        {
            review.PhotoPaths = string.Join(";", photoPaths);
            await _context.SaveChangesAsync();
        }
        return Json(new { success = true });
    }

    // ═══ API for Reviews Tab ═══
    [HttpGet]
    public async Task<IActionResult> GetByLocation([FromQuery] int locationId)
    {
        var reviews = await _reviewService.GetByLocationAsync(locationId);
        var data = reviews.Select(r => new {
            id = r.Id,
            userName = r.User.Name,
            userAvatar = r.User.AvatarPath,
            locationId = r.LocationId,
            rating = r.Rating,
            text = r.Text,
            photoPaths = r.PhotoPaths,
            createdAt = r.CreatedAt,
            ownerResponse = r.OwnerResponse,
            responseDate = r.ResponseDate
        }).ToList();
        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> GetReviews([FromQuery] string? searchLocation = null, [FromQuery] int? userId = null)
    {
        var reviews = await _reviewService.GetFilteredAsync(searchLocation, userId);
        return Json(reviews.Select(r => new
        {
            r.Id,
            r.UserName,
            r.UserAvatar,
            r.LocationId,
            r.LocationName,
            r.Rating,
            r.Text,
            r.PhotoPaths,
            r.CreatedAt,
            r.OwnerResponse,
            r.ResponseDate
        }));
    }

    [HttpGet]
    public async Task<IActionResult> GetMyReviews()
    {
        var currentUserId = AccountController.GetUserId(HttpContext);
        if (!currentUserId.HasValue) return Json(new List<object>());
        return await GetReviews(null, currentUserId.Value);
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateReview(int id, int rating, string text)
    {
        var userId = AccountController.GetUserId(HttpContext);
        if (!userId.HasValue) return Unauthorized();
        var success = await _reviewService.UpdateAsync(id, userId.Value, rating, text);
        if (!success) return Forbid();
        return Ok(new { success = true });
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = AccountController.GetUserId(HttpContext);
        if (!userId.HasValue) return Unauthorized();
        var success = await _reviewService.DeleteAsync(id, userId.Value);
        if (!success) return Forbid();
        return Ok(new { success = true });
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
