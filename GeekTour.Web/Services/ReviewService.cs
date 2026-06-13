using GeekTour.Web.Data;
using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public ReviewService(AppDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<Review> CreateAsync(ReviewCreateViewModel model, int userId)
    {
        var photoPaths = new List<string>();
        foreach (var photo in model.Photos)
        {
            var path = await _fileStorage.SaveFileAsync(photo, "reviews");
            if (path != null) photoPaths.Add(path);
        }

        var review = new Review
        {
            UserId = userId,
            LocationId = model.LocationId,
            Rating = model.Rating,
            Text = model.Text,
            PhotoPaths = string.Join(";", photoPaths),
            CreatedAt = DateTime.UtcNow,
            IsModerated = false
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<bool> RespondAsync(int reviewId, int ownerId, string response)
    {
        var review = await _context.Reviews
            .Include(r => r.Location)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null || review.Location.OwnerId != ownerId) return false;

        review.OwnerResponse = response;
        review.ResponseDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModerateAsync(int reviewId, bool approve)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null) return false;

        if (approve)
            review.IsModerated = true;
        else
            _context.Reviews.Remove(review);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Review>> GetPendingModerationAsync()
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Location)
            .Where(r => !r.IsModerated)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Review>> GetByLocationAsync(int locationId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.LocationId == locationId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Review>> GetByOwnerLocationsAsync(int ownerId)
    {
        var locationIds = await _context.Locations
            .Where(l => l.OwnerId == ownerId)
            .Select(l => l.Id)
            .ToListAsync();

        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Location)
            .Where(r => locationIds.Contains(r.LocationId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ReviewViewModel>> GetFilteredAsync(string? searchLocation = null, int? userId = null)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Location)
            .Where(r => r.IsModerated)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(searchLocation))
        {
            var q = searchLocation.ToLower();
            query = query.Where(r => r.Location.Name.ToLower().Contains(q));
        }

        var raw = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.UserId,
                r.User.Name,
                r.User.AvatarPath,
                r.LocationId,
                LocationName = r.Location.Name,
                r.Rating,
                r.Text,
                r.PhotoPaths,
                r.CreatedAt,
                r.OwnerResponse,
                r.ResponseDate
            }).ToListAsync();

        return raw.Select(r => new ReviewViewModel
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.Name,
            UserAvatar = r.AvatarPath,
            LocationId = r.LocationId,
            LocationName = r.LocationName,
            Rating = r.Rating,
            Text = r.Text,
            PhotoPaths = string.IsNullOrEmpty(r.PhotoPaths) ? new List<string>() : r.PhotoPaths.Split(';').ToList(),
            CreatedAt = r.CreatedAt,
            OwnerResponse = r.OwnerResponse,
            ResponseDate = r.ResponseDate
        }).ToList();
    }

    public async Task<bool> UpdateAsync(int reviewId, int userId, int rating, string text)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null || review.UserId != userId) return false;
        review.Rating = rating;
        review.Text = text;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int reviewId, int userId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null || review.UserId != userId) return false;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }
}
