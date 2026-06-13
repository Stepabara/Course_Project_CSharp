namespace GeekTour.Web.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string?> SaveFileAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0) return null;

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{fileName}";
    }

    public async Task<string?> SaveBase64ImageAsync(string base64Data, string folder)
    {
        if (string.IsNullOrEmpty(base64Data)) return null;
        try
        {
            var commaIdx = base64Data.IndexOf(',');
            if (commaIdx >= 0) base64Data = base64Data.Substring(commaIdx + 1);
            var bytes = Convert.FromBase64String(base64Data);
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid():N}.jpg";
            var filePath = Path.Combine(uploadsDir, fileName);
            await File.WriteAllBytesAsync(filePath, bytes);
            return $"/uploads/{folder}/{fileName}";
        }
        catch { return null; }
    }

    public Task<bool> DeleteFileAsync(string path)
    {
        var fullPath = Path.Combine(_env.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
