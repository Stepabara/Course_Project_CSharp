using System.ComponentModel.DataAnnotations;
using GeekTour.Web.Models.Enums;

namespace GeekTour.Web.Models.Entities;

public class Location
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public LocationType Type { get; set; }

    [Required, MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Working hours stored as JSON string: {"monday":"10:00-22:00",...}
    public string WorkingHoursJson { get; set; } = "{}";

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? OwnerId { get; set; }
    public User? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<LocationFandom> LocationFandoms { get; set; } = new List<LocationFandom>();

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Fandom> Fandoms { get; set; } = new List<Fandom>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<LocationImage> Images { get; set; } = new List<LocationImage>();
    public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    public ICollection<EventItem> Events { get; set; } = new List<EventItem>();
    public ICollection<MenuCatalogItem> MenuCatalog { get; set; } = new List<MenuCatalogItem>();
    public ICollection<ViewStatistics> ViewStats { get; set; } = new List<ViewStatistics>();

    // Computed
    public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
    public int ReviewCount => Reviews.Count;
}
