using GeekTour.Web.Data;
using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Services;

public class RouteService : IRouteService
{
    private readonly AppDbContext _context;
    private readonly IRouteOptimizer _optimizer;

    public RouteService(AppDbContext context, IRouteOptimizer optimizer)
    {
        _context = context;
        _optimizer = optimizer;
    }

    public async Task<List<RouteListViewModel>> GetUserRoutesAsync(int userId)
    {
        return await _context.Routes
            .Include(r => r.Points)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RouteListViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt,
                TotalDistanceKm = r.TotalDistanceKm,
                EstimatedTimeMinutes = r.EstimatedTimeMinutes,
                PointCount = r.Points.Count,
                IsPublic = r.IsPublic,
                IsFavorite = r.IsFavorite,
                ShareLink = r.ShareLink
            }).ToListAsync();
    }

    public async Task<List<RouteListViewModel>> GetPublicRoutesAsync()
    {
        return await _context.Routes
            .Include(r => r.Points)
            .Where(r => r.IsPublic)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RouteListViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt,
                TotalDistanceKm = r.TotalDistanceKm,
                EstimatedTimeMinutes = r.EstimatedTimeMinutes,
                PointCount = r.Points.Count,
                IsPublic = r.IsPublic,
                ShareLink = r.ShareLink
            }).ToListAsync();
    }

    public async Task<RouteDetailViewModel?> GetByIdAsync(int id)
    {
        var route = await _context.Routes
            .Include(r => r.Points)
            .ThenInclude(rp => rp.Location)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (route == null) return null;

        return new RouteDetailViewModel
        {
            Id = route.Id,
            Name = route.Name,
            Description = route.Description,
            CreatedAt = route.CreatedAt,
            TotalDistanceKm = route.TotalDistanceKm,
            EstimatedTimeMinutes = route.EstimatedTimeMinutes,
            IsPublic = route.IsPublic,
            ShareLink = route.ShareLink,
            Points = route.Points.OrderBy(p => p.OrderIndex).Select(rp => new RoutePointViewModel
            {
                Id = rp.Id,
                LocationId = rp.LocationId,
                LocationName = rp.Location.Name,
                LocationAddress = rp.Location.Address,
                Latitude = rp.Location.Latitude,
                Longitude = rp.Location.Longitude,
                OrderIndex = rp.OrderIndex,
                PlannedVisitTime = rp.PlannedVisitTime,
                TravelTimeFromPreviousMinutes = rp.TravelTimeFromPreviousMinutes
            }).ToList()
        };
    }

    public async Task<TourRoute> CreateAsync(RouteCreateViewModel model, int userId)
    {
        var route = new TourRoute
        {
            UserId = userId,
            Name = model.Name,
            Description = model.Description,
            IsPublic = model.IsPublic,
            CreatedAt = DateTime.UtcNow
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        var order = 0;
        foreach (var locId in model.LocationIds)
        {
            _context.RoutePoints.Add(new RoutePoint
            {
                RouteId = route.Id,
                LocationId = locId,
                OrderIndex = order++
            });
        }

        await _context.SaveChangesAsync();
        return route;
    }

    public async Task<RouteDetailViewModel?> OptimizeAsync(int routeId, int userId)
    {
        var route = await _context.Routes
            .Include(r => r.Points)
            .FirstOrDefaultAsync(r => r.Id == routeId && r.UserId == userId);

        if (route == null) return null;

        var locations = await _context.Locations
            .Include(l => l.Reviews)
            .Where(l => route.Points.Select(rp => rp.LocationId).Contains(l.Id))
            .ToListAsync();

        var optimized = _optimizer.OptimizeRoute(locations);

        // Reuse existing route — just reorder points
        var order = 0;
        foreach (var loc in optimized)
        {
            var point = route.Points.FirstOrDefault(rp => rp.LocationId == loc.Id);
            if (point != null)
            {
                point.OrderIndex = order;
                point.TravelTimeFromPreviousMinutes = order > 0
                    ? _optimizer.EstimateTravelTimeMinutes(_optimizer.CalculateDistance(
                        optimized[order - 1].Latitude, optimized[order - 1].Longitude,
                        loc.Latitude, loc.Longitude))
                    : 0;
            }
            order++;
        }

        await _context.SaveChangesAsync();
        return await GetByIdAsync(route.Id);
    }

    public async Task<TourRoute?> OptimizeNewRoute(List<int> locationIds, int userId)
    {
        var locations = await _context.Locations
            .Include(l => l.Reviews)
            .Where(l => locationIds.Contains(l.Id))
            .ToListAsync();

        var optimized = _optimizer.OptimizeRoute(locations);

        var route = new TourRoute
        {
            UserId = userId,
            Name = "Оптимизированный маршрут",
            Description = "Автоматически оптимизированный маршрут",
            CreatedAt = DateTime.UtcNow
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        double totalDist = 0;
        var totalTime = 0;

        for (var i = 0; i < optimized.Count; i++)
        {
            var travelTime = 0;
            if (i > 0)
            {
                var dist = _optimizer.CalculateDistance(
                    optimized[i - 1].Latitude, optimized[i - 1].Longitude,
                    optimized[i].Latitude, optimized[i].Longitude);
                totalDist += dist;
                travelTime = _optimizer.EstimateTravelTimeMinutes(dist);
                totalTime += travelTime;
            }

            _context.RoutePoints.Add(new RoutePoint
            {
                RouteId = route.Id,
                LocationId = optimized[i].Id,
                OrderIndex = i,
                TravelTimeFromPreviousMinutes = travelTime
            });

            totalTime += 60; // Assume 1 hour visit per location
        }

        route.TotalDistanceKm = Math.Round(totalDist, 2);
        route.EstimatedTimeMinutes = totalTime;
        await _context.SaveChangesAsync();

        return route;
    }

    public async Task<bool> SetPublicAsync(int routeId, int userId, bool isPublic)
    {
        var route = await _context.Routes.FirstOrDefaultAsync(r => r.Id == routeId && r.UserId == userId);
        if (route == null) return false;

        route.IsPublic = isPublic;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GenerateShareLinkAsync(int routeId, int userId)
    {
        var route = await _context.Routes.FirstOrDefaultAsync(r => r.Id == routeId && r.UserId == userId);
        if (route == null) return null;

        route.ShareLink = Guid.NewGuid().ToString("N")[..12];
        route.IsPublic = true;
        await _context.SaveChangesAsync();
        return route.ShareLink;
    }

    public async Task<RouteDetailViewModel?> GetByShareLinkAsync(string shareLink)
    {
        var route = await _context.Routes
            .Include(r => r.Points)
            .ThenInclude(rp => rp.Location)
            .FirstOrDefaultAsync(r => r.ShareLink == shareLink && r.IsPublic);

        if (route == null) return null;

        return new RouteDetailViewModel
        {
            Id = route.Id,
            Name = route.Name,
            Description = route.Description,
            CreatedAt = route.CreatedAt,
            TotalDistanceKm = route.TotalDistanceKm,
            EstimatedTimeMinutes = route.EstimatedTimeMinutes,
            IsPublic = route.IsPublic,
            ShareLink = route.ShareLink,
            Points = route.Points.OrderBy(p => p.OrderIndex).Select(rp => new RoutePointViewModel
            {
                Id = rp.Id,
                LocationId = rp.LocationId,
                LocationName = rp.Location.Name,
                LocationAddress = rp.Location.Address,
                Latitude = rp.Location.Latitude,
                Longitude = rp.Location.Longitude,
                OrderIndex = rp.OrderIndex,
                PlannedVisitTime = rp.PlannedVisitTime,
                TravelTimeFromPreviousMinutes = rp.TravelTimeFromPreviousMinutes
            }).ToList()
        };
    }

    public async Task<bool> ToggleFavoriteAsync(int routeId, int userId)
    {
        var route = await _context.Routes.FirstOrDefaultAsync(r => r.Id == routeId && r.UserId == userId);
        if (route == null) return false;
        route.IsFavorite = !route.IsFavorite;
        await _context.SaveChangesAsync();
        return route.IsFavorite;
    }
}
