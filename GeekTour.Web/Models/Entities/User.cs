using System.ComponentModel.DataAnnotations;
using GeekTour.Web.Models.Enums;

namespace GeekTour.Web.Models.Entities;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Tourist;

    // Partner-specific fields
    [MaxLength(200)]
    public string? CompanyName { get; set; }

    [MaxLength(12)]
    public string? INN { get; set; }

    public bool IsVerified { get; set; }

    [MaxLength(500)]
    public string? AvatarPath { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<TourRoute> Routes { get; set; } = new List<TourRoute>();
    public ICollection<Location> OwnedLocations { get; set; } = new List<Location>();
}
