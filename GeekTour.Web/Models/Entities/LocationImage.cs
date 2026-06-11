namespace GeekTour.Web.Models.Entities;

public class LocationImage
{
    public int Id { get; set; }

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public string ImagePath { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}
