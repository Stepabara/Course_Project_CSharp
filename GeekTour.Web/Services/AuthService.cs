using GeekTour.Web.Data;
using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.Enums;
using GeekTour.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model)
    {
        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            return (false, "Пользователь с таким email уже существует", null);

        var user = new User
        {
            Email = model.Email,
            PasswordHash = HashPassword(model.Password),
            Name = model.Name,
            Role = model.IsPartner ? UserRole.Partner : UserRole.Tourist,
            CompanyName = model.CompanyName,
            INN = model.INN,
            IsVerified = !model.IsPartner // Partners need verification
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, "Регистрация успешна", user);
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null)
            return (false, "Неверный email или пароль", null);

        if (!VerifyPassword(model.Password, user.PasswordHash))
            return (false, "Неверный email или пароль", null);

        return (true, "Вход выполнен", user);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "GeekTourSalt2024"));
        return Convert.ToBase64String(bytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
