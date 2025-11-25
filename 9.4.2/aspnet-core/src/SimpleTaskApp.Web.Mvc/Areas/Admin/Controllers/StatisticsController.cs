using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.Controllers;
using SimpleTaskApp.Statistics;
using SimpleTaskApp.Statistics.Dto;
using System.Threading.Tasks;
using System;
using SimpleTaskApp.Web.Areas.Admin.Models.Statistics;

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
    public async Task<IActionResult> ExportTopProductsExcel(string startDate = null, string endDate = null)
    {
      // Tạo filter giống dashboard
      var filter = new StatisticsFilterDto
      {
        StartDate = startDate,
        EndDate = endDate
      };

      // Lấy danh sách TopProducts theo filter
      var stats = await _statisticsAppService.GetDashboardStatisticsAsync(filter);
      var topProducts = stats.TopProducts;

      // Xuất Excel, truyền luôn filter để hiển thị khoảng thời gian trong file
      var fileContents = await _statisticsAppService.ExportTopProductsToExcelAsync(topProducts, filter);

      // Tạo tên file với ngày hiện tại
      var fileName = $"TopProducts_{DateTime.Now:yyyyMMdd}.xlsx";

      return File(fileContents,
                  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                  fileName);
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

      var fileName = $"DashboardStatistics_{DateTime.Now:yyyyMMdd}.xlsx";

      return File(fileContents,
                  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                  fileName);
    }
    // Load partial theo loại thống kê
    // Load partial theo loại thống kê
    public async Task<IActionResult> LoadPartial(string type, StatisticsFilterDto filter)
    {
      filter ??= new StatisticsFilterDto(); // đảm bảo filter không null

      if (type == "LowStock")
      {
        filter.StartDate = null;
        filter.EndDate = null;

        var lowStockData = await _statisticsAppService.GetLowStockProductsAsync(20);
        return PartialView("Partials/_LowStock", lowStockData);
      }
      else
      {
        if (string.IsNullOrWhiteSpace(filter.StartDate) || string.IsNullOrWhiteSpace(filter.EndDate))
        {
          return Content("Vui lòng chọn khoảng thời gian để xem thống kê.");
        }

        var stats = await _statisticsAppService.GetDashboardStatisticsAsync(filter);
        stats.Filter = filter;

        if (type == "Overview")
          return PartialView("Partials/_InfoBoxes", stats);
        else if (type == "TopProducts")
        {
          var viewModel = new TopProductsViewModel
          {
            TopProducts = stats.TopProducts,
            Filter = filter
          };
          return PartialView("Partials/_TopProducts", viewModel);
        }
        else if (type == "BrandRevenue")
          return PartialView("Partials/_BrandRevenue", stats.RevenueByBrandPerCategory);
        else if (type == "TopCustomers")
          return PartialView("Partials/_TopCustomers", stats.TopCustomers);
        else
          return Content("Không tìm thấy loại thống kê.");
      }
    }

  }
}
