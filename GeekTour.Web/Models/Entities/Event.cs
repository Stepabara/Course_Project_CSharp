using System.ComponentModel.DataAnnotations;

namespace GeekTour.Web.Models.Entities;

public class EventItem
{
    public int Id { get; set; }

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    [MaxLength(500)]
    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;
}
