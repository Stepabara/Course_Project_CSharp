using GeekTour.Web.Models.ViewModels;
using GeekTour.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeekTour.Web.Controllers;

public class ReportController : Controller
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public ReportController(IReportService reportService, IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    [HttpGet]
    public IActionResult Index() => View(new ReportFilterViewModel());

    [HttpPost]
    public async Task<IActionResult> CategoryStats(ReportFilterViewModel filter)
    {
        var data = await _reportService.GetCategoryStatsAsync(filter.StartDate, filter.EndDate);
        ViewBag.ReportTitle = "Статистика по категориям";
        ViewBag.Filter = filter;
        return View("CategoryStats", data);
    }

    [HttpPost]
    public async Task<IActionResult> TopLocations(ReportFilterViewModel filter)
    {
        var data = await _reportService.GetTopLocationsAsync(filter.StartDate, filter.EndDate);
        ViewBag.ReportTitle = "Заведения с лучшими отзывами";
        ViewBag.Filter = filter;
        return View("TopLocations", data);
    }

    [HttpPost]
    public async Task<IActionResult> PopularRoutePoints(ReportFilterViewModel filter)
    {
        var data = await _reportService.GetPopularRoutePointsAsync(filter.StartDate, filter.EndDate);
        ViewBag.ReportTitle = "Популярные точки в маршрутах";
        ViewBag.Filter = filter;
        return View("PopularRoutePoints", data);
    }

    [HttpPost]
    public async Task<IActionResult> MonthlyStats()
    {
        var data = await _reportService.GetMonthlyStatsAsync(12);
        ViewBag.ReportTitle = "Статистика по месяцам";
        return View("MonthlyStats", data);
    }

    [HttpPost]
    public async Task<IActionResult> LocationLoad(int locationId, ReportFilterViewModel filter)
    {
        var data = await _reportService.GetLocationLoadAsync(locationId, filter.StartDate, filter.EndDate);
        ViewBag.ReportTitle = "Нагрузка заведения (пиковые часы)";
        ViewBag.Filter = filter;
        ViewBag.LocationId = locationId;
        return View("LocationLoad", data);
    }

    // Export endpoints
    [HttpPost]
    public async Task<IActionResult> ExportCategoryStats(ReportFilterViewModel filter)
    {
        var data = await _reportService.GetCategoryStatsAsync(filter.StartDate, filter.EndDate);
        var bytes = await _exportService.ExportToExcelAsync(data, "Статистика по категориям");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CategoryStats.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> ExportTopLocations(ReportFilterViewModel filter)
    {
        var data = await _reportService.GetTopLocationsAsync(filter.StartDate, filter.EndDate);
        var bytes = await _exportService.ExportToExcelAsync(data, "Топ заведений");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TopLocations.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> ExportMonthlyStats()
    {
        var data = await _reportService.GetMonthlyStatsAsync(12);
        var bytes = await _exportService.ExportToExcelAsync(data, "Статистика по месяцам");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MonthlyStats.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> ExportPopularRoutePoints(ReportFilterViewModel filter)
    {
        var data = await _reportService.GetPopularRoutePointsAsync(filter.StartDate, filter.EndDate);
        var bytes = await _exportService.ExportToExcelAsync(data, "Популярные точки маршрутов");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PopularRoutePoints.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> ExportLocationLoad(int locationId, ReportFilterViewModel filter)
    {
        var data = await _reportService.GetLocationLoadAsync(locationId, filter.StartDate, filter.EndDate);
        var bytes = await _exportService.ExportToExcelAsync(data, "Нагрузка заведения");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LocationLoad.xlsx");
    }
}
