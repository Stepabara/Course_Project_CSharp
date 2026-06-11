namespace GeekTour.Web.Models.Entities;

public class RoutePoint
{
    public int Id { get; set; }

    public int RouteId { get; set; }
    public TourRoute Route { get; set; } = null!;

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public int OrderIndex { get; set; }
    public DateTime? PlannedVisitTime { get; set; }
    public int TravelTimeFromPreviousMinutes { get; set; }
}
