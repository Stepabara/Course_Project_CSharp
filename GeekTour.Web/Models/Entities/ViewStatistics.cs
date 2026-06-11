namespace GeekTour.Web.Models.Entities;

public class ViewStatistics
{
    public int Id { get; set; }

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public DateTime ViewDate { get; set; } = DateTime.UtcNow;

    public int? UserId { get; set; }
    public User? User { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? SearchQuery { get; set; }
}
