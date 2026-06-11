using GeekTour.Web.Data;
using GeekTour.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryStatsViewModel>> GetCategoryStatsAsync(DateTime startDate, DateTime endDate)
    {
        var views = await _context.ViewStatistics
            .Where(vs => vs.ViewDate >= startDate && vs.ViewDate <= endDate)
            .Include(vs => vs.Location)
            .ThenInclude(l => l.Category)
            .ToListAsync();

        var searches = views.Where(vs => !string.IsNullOrEmpty(vs.SearchQuery)).ToList();

        return views
            .GroupBy(vs => vs.Location.Category.Name)
            .Select(g => new CategoryStatsViewModel
            {
                CategoryName = g.Key,
                ViewCount = g.Count(),
                SearchCount = searches.Count(s => s.Location.Category.Name == g.Key),
                LocationCount = g.Select(vs => vs.LocationId).Distinct().Count()
            })
            .OrderByDescending(x => x.ViewCount)
            .ToList();
    }

    public async Task<List<TopLocationViewModel>> GetTopLocationStatsAsync(DateTime startDate, DateTime endDate, int count = 10)
    {
        return await GetTopLocationsAsync(startDate, endDate, count);
    }

    public async Task<List<TopLocationViewModel>> GetTopLocationsAsync(DateTime startDate, DateTime endDate, int count = 10)
    {
        var reviews = await _context.Reviews
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate && r.IsModerated)
            .Include(r => r.Location)
            .ThenInclude(l => l.Category)
            .ToListAsync();

        return reviews
            .GroupBy(r => r.LocationId)
            .Select(g => new TopLocationViewModel
            {
                LocationId = g.First().Location.Id,
                LocationName = g.First().Location.Name,
                AverageRating = Math.Round(g.Average(r => r.Rating), 2),
                ReviewCount = g.Count(),
                CategoryName = g.First().Location.Category.Name
            })
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.ReviewCount)
            .Take(count)
            .ToList();
    }

    public async Task<List<PopularRoutePointViewModel>> GetPopularRoutePointsAsync(DateTime startDate, DateTime endDate)
    {
        var points = await _context.RoutePoints
            .Include(rp => rp.Route)
            .Include(rp => rp.Location)
            .Where(rp => rp.Route.CreatedAt >= startDate && rp.Route.CreatedAt <= endDate)
            .ToListAsync();

        var totalRoutes = points.Select(p => p.RouteId).Distinct().Count();

        return points
            .GroupBy(rp => rp.LocationId)
            .Select(g => new PopularRoutePointViewModel
            {
                LocationId = g.First().Location.Id,
                LocationName = g.First().Location.Name,
                TimesInRoutes = g.Count(),
                PercentageOfRoutes = totalRoutes > 0 ? Math.Round((double)g.Count() / totalRoutes * 100, 1) : 0
            })
            .OrderByDescending(x => x.TimesInRoutes)
            .ToList();
    }

    public async Task<List<MonthlyStatsViewModel>> GetMonthlyStatsAsync(int months = 12)
    {
        var startDate = DateTime.UtcNow.AddMonths(-months);
        var result = new List<MonthlyStatsViewModel>();

        for (var i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);

            var registrations = await _context.Users.CountAsync(u => u.RegisteredAt >= monthStart && u.RegisteredAt < monthEnd);
            var reviews = await _context.Reviews.CountAsync(r => r.CreatedAt >= monthStart && r.CreatedAt < monthEnd);
            var routes = await _context.Routes.CountAsync(r => r.CreatedAt >= monthStart && r.CreatedAt < monthEnd);

            result.Add(new MonthlyStatsViewModel
            {
                Month = monthStart.ToString("MMM yyyy", new System.Globalization.CultureInfo("ru-RU")),
                Registrations = registrations,
                Reviews = reviews,
                Routes = routes
            });
        }

        return result;
    }

    public async Task<List<LocationLoadViewModel>> GetLocationLoadAsync(int locationId, DateTime startDate, DateTime endDate)
    {
        var points = await _context.RoutePoints
            .Include(rp => rp.Route)
            .Include(rp => rp.Location)
            .Where(rp => rp.LocationId == locationId && rp.Route.CreatedAt >= startDate && rp.Route.CreatedAt <= endDate)
            .ToListAsync();

        return points
            .Where(rp => rp.PlannedVisitTime.HasValue)
            .GroupBy(rp => rp.PlannedVisitTime!.Value.Hour)
            .Select(g => new LocationLoadViewModel
            {
                LocationName = g.First().Location?.Name ?? "Неизвестно",
                Hour = g.Key,
                VisitCount = g.Count()
            })
            .OrderBy(x => x.Hour)
            .ToList();
    }

    public async Task<WeightCoefficientsViewModel> GetWeightCoefficientsAsync()
    {
        // In production, load from config/DB. Using defaults for now.
        return new WeightCoefficientsViewModel
        {
            DistanceWeight = 0.4,
            TimeWeight = 0.3,
            RatingWeight = 0.3
        };
    }

    public Task SetWeightCoefficientsAsync(WeightCoefficientsViewModel model)
    {
        // In production, save to config/DB
        return Task.CompletedTask;
    }
}
