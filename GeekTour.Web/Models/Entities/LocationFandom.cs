namespace GeekTour.Web.Models.Entities;

public class LocationFandom
{
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public int FandomId { get; set; }
    public Fandom Fandom { get; set; } = null!;
}
