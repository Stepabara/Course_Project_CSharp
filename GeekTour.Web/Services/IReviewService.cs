using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.ViewModels;

namespace GeekTour.Web.Services;

public interface IReviewService
{
    Task<Review> CreateAsync(ReviewCreateViewModel model, int userId);
    Task<bool> RespondAsync(int reviewId, int ownerId, string response);
    Task<bool> ModerateAsync(int reviewId, bool approve);
    Task<List<Review>> GetPendingModerationAsync();
    Task<List<Review>> GetByLocationAsync(int locationId);
    Task<List<Review>> GetByOwnerLocationsAsync(int ownerId);
    Task<List<ReviewViewModel>> GetFilteredAsync(string? searchLocation = null, int? userId = null);
    Task<bool> UpdateAsync(int reviewId, int userId, int rating, string text);
    Task<bool> DeleteAsync(int reviewId, int userId);
}
