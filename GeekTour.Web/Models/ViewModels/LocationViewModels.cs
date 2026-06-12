using System.ComponentModel.DataAnnotations;
using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.Enums;

namespace GeekTour.Web.Models.ViewModels;

public class LocationListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string? MainImagePath { get; set; }
    public List<int> FandomIds { get; set; } = new();
    public List<string> FandomNames { get; set; } = new();
    public string CategoryName { get; set; } = string.Empty;
    public string WorkingHoursJson { get; set; } = "{}";
    public List<Promotion> ActivePromotions { get; set; } = new();
}

public class LocationDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string WorkingHoursJson { get; set; } = "{}";
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public List<Fandom> Fandoms { get; set; } = new();
    public List<string> ImagePaths { get; set; } = new();
    public List<ReviewViewModel> Reviews { get; set; } = new();
    public List<Promotion> ActivePromotions { get; set; } = new();
    public List<EventItem> UpcomingEvents { get; set; } = new();
    public List<MenuCatalogItem> MenuCatalog { get; set; } = new();
}

public class LocationEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public LocationType Type { get; set; }

    [Required(ErrorMessage = "Введите адрес")]
    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string WorkingHoursJson { get; set; } = "{}";

    [Required]
    public int CategoryId { get; set; }

    public List<int> FandomIds { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class LocationFilterViewModel
{
    public string? SearchQuery { get; set; }
    public LocationType? Type { get; set; }
    public int? CategoryId { get; set; }
    public int? FandomId { get; set; }
    public List<int> FandomIds { get; set; } = new();
    public double? MinRating { get; set; }
    public string SortBy { get; set; } = "rating"; // rating, name, newest
}
