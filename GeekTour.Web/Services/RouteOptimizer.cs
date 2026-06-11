using GeekTour.Web.Models.Entities;

namespace GeekTour.Web.Services;

public class RouteOptimizer : IRouteOptimizer
{
    public List<Location> OptimizeRoute(List<Location> locations, double distanceWeight = 0.4, double timeWeight = 0.3, double ratingWeight = 0.3)
    {
        if (locations.Count <= 2) return locations.ToList();

        // Phase 1: Nearest Neighbor heuristic
        var ordered = NearestNeighbor(locations, distanceWeight, ratingWeight);

        // Phase 2: 2-opt improvement
        ordered = TwoOptImprove(ordered);

        // Phase 3: Reorder based on working hours (if location arrives when closed, move it later)
        if (timeWeight > 0)
        {
            ordered = ReorderByWorkingHours(ordered);
        }

        return ordered;
    }

    /// <summary>
    /// Reorder route points considering working hours.
    /// If a location would be arrived at when it's closed, try to move it to a later position.
    /// </summary>
    private List<Location> ReorderByWorkingHours(List<Location> route)
    {
        if (route.Count <= 2) return route;

        var result = new List<Location> { route[0] };
        var currentTime = DateTime.Today.AddHours(10); // Start at 10:00
        var remaining = route.Skip(1).ToList();

        while (remaining.Count > 0)
        {
            Location? best = null;
            var bestIdx = -1;

            for (var i = 0; i < remaining.Count; i++)
            {
                var loc = remaining[i];
                var travelMin = result.Count > 0
                    ? EstimateTravelTimeMinutes(CalculateDistance(
                        result[^1].Latitude, result[^1].Longitude,
                        loc.Latitude, loc.Longitude))
                    : 0;

                var arrivalTime = currentTime.AddMinutes(travelMin);
                var arrivalHour = arrivalTime.Hour;
                var dayOfWeek = arrivalTime.DayOfWeek.ToString().ToLower();

                // Check if location is open at arrival time
                if (IsLocationOpen(loc, dayOfWeek, arrivalHour))
                {
                    best = loc;
                    bestIdx = i;
                    break; // Take first open location
                }

                // If none found yet, pick the one that opens soonest
                if (best == null)
                {
                    best = loc;
                    bestIdx = i;
                }
            }

            if (best != null)
            {
                var travelMin = EstimateTravelTimeMinutes(CalculateDistance(
                    result[^1].Latitude, result[^1].Longitude,
                    best.Latitude, best.Longitude));
                currentTime = currentTime.AddMinutes(travelMin).AddMinutes(60); // +1h visit
                result.Add(best);
                remaining.RemoveAt(bestIdx);
            }
            else break;
        }

        return result;
    }

    /// <summary>
    /// Check if a location is open on given day and hour
    /// </summary>
    private bool IsLocationOpen(Location location, string dayOfWeek, int hour)
    {
        if (string.IsNullOrEmpty(location.WorkingHoursJson))
            return hour >= 9 && hour <= 22; // Default: 9-22

        try
        {
            var wh = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(location.WorkingHoursJson);
            if (wh == null || !wh.TryGetValue(dayOfWeek, out var hours))
                return hour >= 9 && hour <= 22;

            if (hours == "closed") return false;

            var parts = hours.Split('-');
            if (parts.Length != 2) return hour >= 9 && hour <= 22;

            if (int.TryParse(parts[0].Split(':')[0], out var openHour) &&
                int.TryParse(parts[1].Split(':')[0], out var closeHour))
            {
                if (closeHour < openHour) // Overnight (e.g., 14:00-02:00)
                    return hour >= openHour || hour < closeHour;
                return hour >= openHour && hour < closeHour;
            }
        }
        catch { }

        return hour >= 9 && hour <= 22;
    }

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    public int EstimateTravelTimeMinutes(double distanceKm)
    {
        const double avgSpeedKmh = 30; // City average
        return (int)Math.Ceiling(distanceKm / avgSpeedKmh * 60);
    }

    private List<Location> NearestNeighbor(List<Location> locations, double distanceWeight, double ratingWeight)
    {
        var remaining = new List<Location>(locations);
        var route = new List<Location>();

        // Start from the location with highest rating
        var start = remaining.OrderByDescending(l =>
        {
            var avgRating = l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : 3.0;
            return avgRating;
        }).First();

        route.Add(start);
        remaining.Remove(start);

        while (remaining.Count > 0)
        {
            var last = route[^1];
            Location? next = null;
            var bestScore = double.MaxValue;

            foreach (var candidate in remaining)
            {
                var dist = CalculateDistance(last.Latitude, last.Longitude, candidate.Latitude, candidate.Longitude);
                var avgRating = candidate.Reviews.Any() ? candidate.Reviews.Average(r => (double)r.Rating) : 3.0;
                var score = distanceWeight * dist - ratingWeight * avgRating;

                if (score < bestScore)
                {
                    bestScore = score;
                    next = candidate;
                }
            }

            if (next != null)
            {
                route.Add(next);
                remaining.Remove(next);
            }
            else break;
        }

        return route;
    }

    private List<Location> TwoOptImprove(List<Location> route)
    {
        var improved = true;
        var best = route.ToList();

        while (improved)
        {
            improved = false;
            for (var i = 0; i < best.Count - 1; i++)
            {
                for (var j = i + 2; j < best.Count; j++)
                {
                    var newRoute = TwoOptSwap(best, i, j);
                    if (TotalDistance(newRoute) < TotalDistance(best))
                    {
                        best = newRoute;
                        improved = true;
                    }
                }
            }
        }

        return best;
    }

    private List<Location> TwoOptSwap(List<Location> route, int i, int j)
    {
        var newRoute = new List<Location>();

        for (var k = 0; k <= i; k++)
            newRoute.Add(route[k]);

        for (var k = j; k >= i + 1; k--)
            newRoute.Add(route[k]);

        for (var k = j + 1; k < route.Count; k++)
            newRoute.Add(route[k]);

        return newRoute;
    }

    private double TotalDistance(List<Location> route)
    {
        var total = 0.0;
        for (var i = 0; i < route.Count - 1; i++)
        {
            total += CalculateDistance(route[i].Latitude, route[i].Longitude, route[i + 1].Latitude, route[i + 1].Longitude);
        }
        return total;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
