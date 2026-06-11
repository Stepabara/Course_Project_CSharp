using GeekTour.Web.Models.Entities;

namespace GeekTour.Web.Services;

public interface IRouteOptimizer
{
    /// <summary>
    /// Optimizes route order using Nearest Neighbor + 2-opt, considering working hours
    /// </summary>
    List<Location> OptimizeRoute(List<Location> locations, double distanceWeight = 0.4, double timeWeight = 0.3, double ratingWeight = 0.3);

    /// <summary>
    /// Calculates distance between two points using Haversine formula (km)
    /// </summary>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Estimated travel time in minutes (assuming average speed ~30 km/h in city)
    /// </summary>
    int EstimateTravelTimeMinutes(double distanceKm);
}
