using System.ComponentModel.DataAnnotations;

namespace GeekTour.Web.Models.Entities;

public class MenuCatalogItem
{
    public int Id { get; set; }

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? ImagePath { get; set; }
}
