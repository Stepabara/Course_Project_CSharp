using GeekTour.Web.Data;
using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.Enums;
using GeekTour.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _context;

    public LocationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LocationListViewModel>> GetFilteredAsync(LocationFilterViewModel filter)
    {
        // Load all data into memory first, then filter client-side
        // to avoid EF Core / SQLite translation issues with complex queries
        var allLocations = await _context.Locations
            .Include(l => l.Category)
            .Include(l => l.LocationFandoms).ThenInclude(lf => lf.Fandom)
            .Include(l => l.Images)
            .Include(l => l.Promotions)
            .Include(l => l.Reviews)
            .Where(l => l.IsActive)
            .ToListAsync();

        var query = allLocations.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var q = filter.SearchQuery.ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(q)
                || l.Description.ToLower().Contains(q)
                || l.Address.ToLower().Contains(q));
        }

        if (filter.Type.HasValue)
            query = query.Where(l => l.Type == filter.Type.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(l => l.CategoryId == filter.CategoryId.Value);

        if (filter.FandomId.HasValue)
            query = query.Where(l => l.LocationFandoms.Any(lf => lf.FandomId == filter.FandomId.Value));

        if (filter.MinRating.HasValue)
            query = query.Where(l => l.Reviews.Any() && l.Reviews.Average(r => r.Rating) >= filter.MinRating.Value);

        query = filter.SortBy switch
        {
            "name" => query.OrderBy(l => l.Name),
            "newest" => query.OrderByDescending(l => l.CreatedAt),
            _ => query.OrderByDescending(l => l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : 0)
        };

        return query.Select(l => new LocationListViewModel
        {
            Id = l.Id,
            Name = l.Name,
            Description = l.Description.Length > 150 ? l.Description[..150] + "..." : l.Description,
            Type = l.Type,
            Address = l.Address,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            AverageRating = l.AverageRating,
            ReviewCount = l.ReviewCount,
            MainImagePath = l.Images.FirstOrDefault(i => i.IsMain)?.ImagePath ?? l.Images.FirstOrDefault()?.ImagePath,
            FandomNames = l.LocationFandoms.Select(lf => lf.Fandom.Name).ToList(),
            CategoryName = l.Category.Name,
            ActivePromotions = l.Promotions.Where(p => p.IsActive).ToList()
        }).ToList();
    }

    public async Task<LocationDetailViewModel?> GetByIdAsync(int id, int? userId = null)
    {
        var location = await _context.Locations
            .Include(l => l.Category)
            .Include(l => l.LocationFandoms).ThenInclude(lf => lf.Fandom)
            .Include(l => l.Images)
            .Include(l => l.Promotions)
            .Include(l => l.Events)
            .Include(l => l.MenuCatalog)
            .Include(l => l.Reviews).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location == null) return null;

        // Only record view statistics for a real, existing user (skip userId 0 from hardcoded admin)
        if (userId.HasValue && userId.Value > 0 && await _context.Users.AnyAsync(u => u.Id == userId.Value))
        {
            _context.ViewStatistics.Add(new ViewStatistics
            {
                LocationId = id,
                UserId = userId,
                ViewDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        var fandoms = location.LocationFandoms.Select(lf => lf.Fandom).ToList();

        return new LocationDetailViewModel
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Type = location.Type,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            WorkingHoursJson = location.WorkingHoursJson,
            AverageRating = location.AverageRating,
            ReviewCount = location.ReviewCount,
            CategoryName = location.Category.Name,
            CategoryId = location.CategoryId,
            Fandoms = fandoms,
            ImagePaths = location.Images.Select(i => i.ImagePath).ToList(),
            Reviews = location.Reviews.OrderByDescending(r => r.CreatedAt).Select(r => new ReviewViewModel
            {
                Id = r.Id,
                UserName = r.User.Name,
                Rating = r.Rating,
                Text = r.Text,
                PhotoPaths = string.IsNullOrEmpty(r.PhotoPaths) ? new() : r.PhotoPaths.Split(';').ToList(),
                CreatedAt = r.CreatedAt,
                OwnerResponse = r.OwnerResponse,
                ResponseDate = r.ResponseDate
            }).ToList(),
            ActivePromotions = location.Promotions.Where(p => p.IsActive).ToList(),
            UpcomingEvents = location.Events.Where(e => e.EventDate >= DateTime.UtcNow && e.IsActive).OrderBy(e => e.EventDate).ToList(),
            MenuCatalog = location.MenuCatalog.ToList()
        };
    }

    public async Task<Location> CreateAsync(LocationEditViewModel model, int ownerId)
    {
        var location = new Location
        {
            Name = model.Name,
            Description = model.Description,
            Type = model.Type,
            Address = model.Address,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            WorkingHoursJson = model.WorkingHoursJson,
            CategoryId = model.CategoryId,
            OwnerId = ownerId,
            IsActive = model.IsActive
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        foreach (var fandomId in model.FandomIds ?? new List<int>())
        {
            _context.LocationFandoms.Add(new LocationFandom { LocationId = location.Id, FandomId = fandomId });
        }
        await _context.SaveChangesAsync();

        return location;
    }

    public async Task<bool> UpdateAsync(LocationEditViewModel model, int userId, bool isAdmin)
    {
        var location = await _context.Locations.FindAsync(model.Id);
        if (location == null) return false;
        if (!isAdmin && location.OwnerId != userId) return false;

        location.Name = model.Name;
        location.Description = model.Description;
        location.Type = model.Type;
        location.Address = model.Address;
        location.Latitude = model.Latitude;
        location.Longitude = model.Longitude;
        location.WorkingHoursJson = model.WorkingHoursJson;
        location.CategoryId = model.CategoryId;
        location.IsActive = model.IsActive;

        _context.LocationFandoms.RemoveRange(_context.LocationFandoms.Where(lf => lf.LocationId == location.Id));
        foreach (var fandomId in model.FandomIds ?? new List<int>())
        {
            _context.LocationFandoms.Add(new LocationFandom { LocationId = location.Id, FandomId = fandomId });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int userId, bool isAdmin)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null) return false;
        if (!isAdmin && location.OwnerId != userId) return false;

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Location>> GetByOwnerAsync(int ownerId)
    {
        return await _context.Locations
            .Include(l => l.Category)
            .Include(l => l.Reviews)
            .Where(l => l.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<List<Fandom>> GetAllFandomsAsync()
    {
        return await _context.Fandoms.OrderBy(f => f.Category).ThenBy(f => f.Name).ToListAsync();
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<List<LocationListViewModel>> GetRecommendedAsync(int userId, int count = 5)
    {
        var userLocationIds = await _context.Reviews
            .Where(r => r.UserId == userId && r.Rating >= 4)
            .Select(r => r.LocationId)
            .Distinct()
            .ToListAsync();

        var likedFandomIds = await _context.LocationFandoms
            .Where(lf => userLocationIds.Contains(lf.LocationId))
            .Select(lf => lf.FandomId)
            .Distinct()
            .ToListAsync();

        var visitedIds = await _context.Reviews.Where(r => r.UserId == userId).Select(r => r.LocationId).ToListAsync();

        var recommended = await _context.Locations
            .Include(l => l.LocationFandoms).ThenInclude(lf => lf.Fandom)
            .Include(l => l.Reviews)
            .Include(l => l.Images)
            .Include(l => l.Category)
            .Include(l => l.Promotions)
            .Where(l => l.IsActive && !visitedIds.Contains(l.Id))
            .OrderByDescending(l => l.LocationFandoms.Count(lf => likedFandomIds.Contains(lf.FandomId)) * 10 + l.Reviews.Count)
            .Take(count)
            .ToListAsync();

        return recommended.Select(l => new LocationListViewModel
        {
            Id = l.Id,
            Name = l.Name,
            Description = l.Description.Length > 150 ? l.Description[..150] + "..." : l.Description,
            Type = l.Type,
            Address = l.Address,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            AverageRating = l.AverageRating,
            ReviewCount = l.ReviewCount,
            MainImagePath = l.Images.FirstOrDefault(i => i.IsMain)?.ImagePath ?? l.Images.FirstOrDefault()?.ImagePath,
            FandomNames = l.LocationFandoms.Select(lf => lf.Fandom.Name).ToList(),
            CategoryName = l.Category.Name,
            ActivePromotions = l.Promotions.Where(p => p.IsActive).ToList()
        }).ToList();
    }
}
