namespace GeekTour.Web.Services;

public interface IExportService
{
    Task<byte[]> ExportToExcelAsync<T>(List<T> data, string sheetName) where T : class;
    Task<byte[]> ExportToWordAsync(string title, Dictionary<string, string> content);
}
