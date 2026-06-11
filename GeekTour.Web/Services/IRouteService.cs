using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.ViewModels;

namespace GeekTour.Web.Services;

public interface IRouteService
{
    Task<List<RouteListViewModel>> GetUserRoutesAsync(int userId);
    Task<RouteDetailViewModel?> GetByIdAsync(int id);
    Task<TourRoute> CreateAsync(RouteCreateViewModel model, int userId);
    Task<RouteDetailViewModel?> OptimizeAsync(int routeId, int userId);
    Task<TourRoute?> OptimizeNewRoute(List<int> locationIds, int userId);
    Task<bool> SetPublicAsync(int routeId, int userId, bool isPublic);
    Task<string?> GenerateShareLinkAsync(int routeId, int userId);
    Task<RouteDetailViewModel?> GetByShareLinkAsync(string shareLink);
}
