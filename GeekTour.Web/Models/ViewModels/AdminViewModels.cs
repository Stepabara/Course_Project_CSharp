using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.Enums;

namespace GeekTour.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalLocations { get; set; }
    public int TotalReviews { get; set; }
    public int TotalRoutes { get; set; }
    public int PendingReviews { get; set; }
    public int VerifiedPartners { get; set; }
    public List<User> RecentUsers { get; set; } = new();
    public List<Review> PendingReviewList { get; set; } = new();
}

public class AdminUserViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? CompanyName { get; set; }
    public bool IsVerified { get; set; }
    public DateTime RegisteredAt { get; set; }
    public int LocationCount { get; set; }
}

public class AdminLocationViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public bool IsActive { get; set; }
}

public class FandomEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public FandomCategory Category { get; set; }
}

public class CategoryEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
