using System.ComponentModel.DataAnnotations;

namespace GeekTour.Web.Models.Entities;

public class TourRoute
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublic { get; set; }
    public bool IsFavorite { get; set; }

    [MaxLength(100)]
    public string? ShareLink { get; set; }

    public double TotalDistanceKm { get; set; }
    public int EstimatedTimeMinutes { get; set; }

    // Navigation
    public ICollection<RoutePoint> Points { get; set; } = new List<RoutePoint>();
}
