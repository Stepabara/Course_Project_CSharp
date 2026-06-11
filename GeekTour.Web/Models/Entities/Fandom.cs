using System.ComponentModel.DataAnnotations;
using GeekTour.Web.Models.Enums;

namespace GeekTour.Web.Models.Entities;

public class Fandom
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public FandomCategory Category { get; set; }

    public ICollection<LocationFandom> LocationFandoms { get; set; } = new List<LocationFandom>();

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
