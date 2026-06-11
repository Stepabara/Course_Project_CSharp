using GeekTour.Web.Models.ViewModels;

namespace GeekTour.Web.Services;

public interface IReportService
{
    Task<List<CategoryStatsViewModel>> GetCategoryStatsAsync(DateTime startDate, DateTime endDate);
    Task<List<TopLocationViewModel>> GetTopLocationsAsync(DateTime startDate, DateTime endDate, int count = 10);
    Task<List<PopularRoutePointViewModel>> GetPopularRoutePointsAsync(DateTime startDate, DateTime endDate);
    Task<List<MonthlyStatsViewModel>> GetMonthlyStatsAsync(int months = 12);
    Task<List<LocationLoadViewModel>> GetLocationLoadAsync(int locationId, DateTime startDate, DateTime endDate);
    Task<WeightCoefficientsViewModel> GetWeightCoefficientsAsync();
    Task SetWeightCoefficientsAsync(WeightCoefficientsViewModel model);
}
