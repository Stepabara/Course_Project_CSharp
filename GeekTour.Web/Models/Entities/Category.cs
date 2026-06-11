using System.ComponentModel.DataAnnotations;

namespace GeekTour.Web.Models.Entities;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
