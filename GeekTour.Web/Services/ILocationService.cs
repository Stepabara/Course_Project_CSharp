using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.ViewModels;

namespace GeekTour.Web.Services;

public interface ILocationService
{
    Task<List<LocationListViewModel>> GetFilteredAsync(LocationFilterViewModel filter);
    Task<LocationDetailViewModel?> GetByIdAsync(int id, int? userId = null);
    Task<Location> CreateAsync(LocationEditViewModel model, int ownerId);
    Task<bool> UpdateAsync(LocationEditViewModel model, int userId, bool isAdmin);
    Task<bool> DeleteAsync(int id, int userId, bool isAdmin);
    Task<List<Location>> GetByOwnerAsync(int ownerId);
    Task<List<Fandom>> GetAllFandomsAsync();
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<LocationListViewModel>> GetRecommendedAsync(int userId, int count = 5);
}
