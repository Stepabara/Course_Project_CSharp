using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Reflection;

namespace GeekTour.Web.Services;

public class ExportService : IExportService
{
    public Task<byte[]> ExportToExcelAsync<T>(List<T> data, string sheetName) where T : class
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        if (data.Count == 0)
        {
            return Task.FromResult(GetBytes(workbook));
        }

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Headers
        for (var i = 0; i < properties.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = properties[i].Name;
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Data
        for (var row = 0; row < data.Count; row++)
        {
            for (var col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(data[row]);
                worksheet.Cell(row + 2, col + 1).Value = value?.ToString() ?? "";
            }
        }

        worksheet.Columns().AdjustToContents();
        return Task.FromResult(GetBytes(workbook));
    }

    public Task<byte[]> ExportToWordAsync(string title, Dictionary<string, string> content)
    {
        using var stream = new MemoryStream();
        using var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = new Body();

        // Title
        var titleRunProps = new RunProperties();
        titleRunProps.Append(new Bold());
        titleRunProps.Append(new FontSize { Val = "28" });
        body.Append(new Paragraph(new Run(titleRunProps, new Text(title))));

        // Content
        foreach (var (key, value) in content)
        {
            var labelRunProps = new RunProperties();
            labelRunProps.Append(new Bold());
            body.Append(new Paragraph(
                new Run(labelRunProps, new Text(key + ": ")),
                new Run(new Text(value))));
        }

        mainPart.Document.Append(body);
        mainPart.Document.Save();

        return Task.FromResult(stream.ToArray());
    }

    private static byte[] GetBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
