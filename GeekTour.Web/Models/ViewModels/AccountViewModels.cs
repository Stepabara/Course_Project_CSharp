using System.ComponentModel.DataAnnotations;
using GeekTour.Web.Models.Enums;

namespace GeekTour.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите имя")]
    [Display(Name = "Имя")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Минимум 6 символов")]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Регистрация как партнёр")]
    public bool IsPartner { get; set; }

    [Display(Name = "Название компании")]
    public string? CompanyName { get; set; }

    [Display(Name = "ИНН")]
    [MaxLength(12)]
    public string? INN { get; set; }
}
