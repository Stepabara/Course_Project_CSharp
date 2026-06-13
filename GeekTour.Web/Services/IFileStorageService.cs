namespace GeekTour.Web.Services;

public interface IFileStorageService
{
    Task<string?> SaveFileAsync(IFormFile file, string folder);
    Task<string?> SaveBase64ImageAsync(string base64Data, string folder);
    Task<bool> DeleteFileAsync(string path);
}
