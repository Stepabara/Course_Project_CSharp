namespace GeekTour.Web.Services;

public interface IFileStorageService
{
    Task<string?> SaveFileAsync(IFormFile file, string folder);
    Task<bool> DeleteFileAsync(string path);
}
