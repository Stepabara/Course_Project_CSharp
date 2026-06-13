using System.ComponentModel.DataAnnotations;
using GeekTour.Web.Models.Entities;

namespace GeekTour.Web.Models.ViewModels;

public class RouteListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double TotalDistanceKm { get; set; }
    public int EstimatedTimeMinutes { get; set; }
    public int PointCount { get; set; }
    public bool IsPublic { get; set; }
    public bool IsFavorite { get; set; }
    public string? ShareLink { get; set; }
}

public class RouteDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double TotalDistanceKm { get; set; }
    public int EstimatedTimeMinutes { get; set; }
    public bool IsPublic { get; set; }
    public string? ShareLink { get; set; }
    public List<RoutePointViewModel> Points { get; set; } = new();
}

public class RoutePointViewModel
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int OrderIndex { get; set; }
    public DateTime? PlannedVisitTime { get; set; }
    public int TravelTimeFromPreviousMinutes { get; set; }
}

public class RouteCreateViewModel
{
    [Required(ErrorMessage = "Введите название маршрута")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите хотя бы 2 точки")]
    [MinLength(2, ErrorMessage = "Минимум 2 точки для маршрута")]
    public List<int> LocationIds { get; set; } = new();

    public bool IsPublic { get; set; }
}

public class RouteOptimizeViewModel
{
    public int RouteId { get; set; }
    public List<int> LocationIds { get; set; } = new();
    public DateTime? StartTime { get; set; }
    public bool ConsiderWorkingHours { get; set; } = true;
}
