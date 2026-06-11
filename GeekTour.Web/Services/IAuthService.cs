using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.ViewModels;

namespace GeekTour.Web.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model);
    Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByEmailAsync(string email);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
