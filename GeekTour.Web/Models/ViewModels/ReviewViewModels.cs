using System.ComponentModel.DataAnnotations;
using GeekTour.Web.Models.Entities;

namespace GeekTour.Web.Models.ViewModels;

public class ReviewViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> PhotoPaths { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? OwnerResponse { get; set; }
    public DateTime? ResponseDate { get; set; }
}

public class ReviewCreateViewModel
{
    public int LocationId { get; set; }

    [Required(ErrorMessage = "Укажите оценку")]
    [Range(1, 5, ErrorMessage = "Оценка от 1 до 5")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Напишите текст отзыва")]
    public string Text { get; set; } = string.Empty;

    [Display(Name = "Фотографии")]
    public List<IFormFile> Photos { get; set; } = new();
}

public class ReviewResponseViewModel
{
    public int ReviewId { get; set; }

    [Required(ErrorMessage = "Напишите ответ")]
    public string Response { get; set; } = string.Empty;
}
