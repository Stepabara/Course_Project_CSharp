using System.ComponentModel.DataAnnotations;

namespace GeekTour.Web.Models.Entities;

public class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    // Photo paths stored as semicolon-separated string
    public string PhotoPaths { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsModerated { get; set; }

    // Owner response
    public string? OwnerResponse { get; set; }
    public DateTime? ResponseDate { get; set; }
}
