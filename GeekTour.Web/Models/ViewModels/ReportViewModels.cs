using System.ComponentModel.DataAnnotations;

namespace GeekTour.Web.Models.ViewModels;

public class ReportFilterViewModel
{
    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);

    [Required]
    public DateTime EndDate { get; set; } = DateTime.UtcNow;

    public int? CategoryId { get; set; }
    public int? FandomId { get; set; }
}

public class CategoryStatsViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int SearchCount { get; set; }
    public int LocationCount { get; set; }
}

public class TopLocationViewModel
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class PopularRoutePointViewModel
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int TimesInRoutes { get; set; }
    public double PercentageOfRoutes { get; set; }
}

public class MonthlyStatsViewModel
{
    public string Month { get; set; } = string.Empty;
    public int Registrations { get; set; }
    public int Reviews { get; set; }
    public int Routes { get; set; }
}

public class LocationLoadViewModel
{
    public string LocationName { get; set; } = string.Empty;
    public int Hour { get; set; }
    public int VisitCount { get; set; }
    public string HourLabel => $"{Hour:00}:00 - {Hour + 1:00}:00";
}

public class WeightCoefficientsViewModel
{
    [Range(0, 1)]
    public double DistanceWeight { get; set; } = 0.4;

    [Range(0, 1)]
    public double TimeWeight { get; set; } = 0.3;

    [Range(0, 1)]
    public double RatingWeight { get; set; } = 0.3;
}
