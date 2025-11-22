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

    // Load partial theo loại thống kê
    public async Task<IActionResult> LoadPartial(string type, StatisticsFilterDto filter)
    {
      // LowStock luôn load, không cần filter ngày
      if (type == "LowStock")
      {
        filter.StartDate = null;
        filter.EndDate = null;
      }
      else
      {
        // Nếu không có filter ngày → trả về thông báo
        if (string.IsNullOrWhiteSpace(filter.StartDate) || string.IsNullOrWhiteSpace(filter.EndDate))
        {
          return Content("Vui lòng chọn khoảng thời gian để xem thống kê.");
        }
      }

      var data = await _statisticsAppService.GetDashboardStatisticsAsync(filter);
      data.Filter = filter;

      return type switch
      {
        "Overview" => PartialView("Partials/_InfoBoxes", data),
        "TopProducts" => PartialView("Partials/_TopProducts", data.TopProducts),
        "BrandRevenue" => PartialView("Partials/_BrandRevenue", data.RevenueByBrandPerCategory),
        "LowStock" => PartialView("Partials/_LowStock", data.LowStockProducts),
        "TopCustomers" => PartialView("Partials/_TopCustomers", data.TopCustomers),
        _ => Content("Không tìm thấy loại thống kê.")
      };
    }
  }
}
