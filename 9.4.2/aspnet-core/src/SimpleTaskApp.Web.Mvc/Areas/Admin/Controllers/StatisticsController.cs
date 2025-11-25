using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.Controllers;
using SimpleTaskApp.Statistics;
using SimpleTaskApp.Statistics.Dto;
using System.Threading.Tasks;
using System;

namespace SimpleTaskApp.Areas.Admin.Controllers
{
  [Area("Admin")]
  public class StatisticsController : SimpleTaskAppControllerBase
  {
    private readonly IStatisticsAppService _statisticsAppService;

    public StatisticsController(IStatisticsAppService statisticsAppService)
    {
      _statisticsAppService = statisticsAppService;
    }

    // GET: /Admin/Statistics/Index
    public async Task<IActionResult> Index(StatisticsFilterDto filter)
    {
      // Nếu không có ngày → mặc định là ngày hiện tại
      if (string.IsNullOrWhiteSpace(filter.StartDate) || string.IsNullOrWhiteSpace(filter.EndDate))
      {
        var today = DateTime.Today;
        filter.StartDate = today.ToString("yyyy-MM-dd");
        filter.EndDate = today.ToString("yyyy-MM-dd");
      }

      var stats = await _statisticsAppService.GetDashboardStatisticsAsync(filter);
      stats.Filter = filter;

      return View(stats);
    }
    [HttpGet]
    public async Task<IActionResult> ExportExcel(string startDate = null, string endDate = null)
    {
      // Tạo filter từ query string (giống filter trên dashboard)
      var filter = new StatisticsFilterDto
      {
        StartDate = startDate,
        EndDate = endDate
      };

      var stats = await _statisticsAppService.GetDashboardStatisticsAsync(filter);
      var fileContents = await _statisticsAppService.ExportStatisticsToExcelAsync(stats);

      return File(fileContents,
                  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                  "DashboardStatistics.xlsx");
    }
    // Load partial theo loại thống kê
    public async Task<IActionResult> LoadPartial(string type, StatisticsFilterDto filter)
    {
      filter ??= new StatisticsFilterDto(); // đảm bảo filter không null

      // LowStock luôn load, không cần filter ngày
      if (type == "LowStock")
      {
        filter.StartDate = null;
        filter.EndDate = null;

        // Lấy dữ liệu tồn kho thấp riêng
        var lowStockData = await _statisticsAppService.GetLowStockProductsAsync(20);
        return PartialView("Partials/_LowStock", lowStockData);
      }
      else
      {
        // Nếu không có filter ngày → trả về thông báo
        if (string.IsNullOrWhiteSpace(filter.StartDate) || string.IsNullOrWhiteSpace(filter.EndDate))
        {
          return Content("Vui lòng chọn khoảng thời gian để xem thống kê.");
        }

        // Lấy toàn bộ statistics
        var data = await _statisticsAppService.GetDashboardStatisticsAsync(filter);
        data.Filter = filter;

        if (type == "Overview")
          return PartialView("Partials/_InfoBoxes", data);
        else if (type == "TopProducts")
          return PartialView("Partials/_TopProducts", data.TopProducts);
        else if (type == "BrandRevenue")
          return PartialView("Partials/_BrandRevenue", data.RevenueByBrandPerCategory);
        else if (type == "TopCustomers")
          return PartialView("Partials/_TopCustomers", data.TopCustomers);
        else
          return Content("Không tìm thấy loại thống kê.");
      }
    }
  
  }
}
