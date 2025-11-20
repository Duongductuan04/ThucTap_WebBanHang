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
      // ========================
      // XỬ LÝ KHOẢNG NGÀY MẶC ĐỊNH
      // ========================
      if (string.IsNullOrWhiteSpace(filter.StartDate) || string.IsNullOrWhiteSpace(filter.EndDate))
      {
        // Mặc định tháng hiện tại
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = now; // đến thời điểm hiện tại

        filter.StartDate = startOfMonth.ToString("yyyy-MM-dd");
        filter.EndDate = endOfMonth.ToString("yyyy-MM-dd");
      }

      // ========================
      // Lấy thống kê
      // ========================
      var stats = await _statisticsAppService.GetDashboardStatisticsAsync(filter);

      // Truyền filter xuống View để Date Range Picker hiển thị đúng
      ViewBag.Filter = filter;

      // Trả về View
      return View(stats);
    }


    // 🔥 Load partial theo select dropdown
    public async Task<IActionResult> LoadPartial(string type, StatisticsFilterDto filter)
    {
      // Luôn phải load lại thống kê dựa vào filter
      var data = await _statisticsAppService.GetDashboardStatisticsAsync(filter);

      switch (type)
      {
        case "TopProducts":
          return PartialView("Partials/_TopProducts", data.TopProducts);

        case "BrandRevenue":
          return PartialView("Partials/_BrandRevenue", data.RevenueByBrandPerCategory);

        case "LowStock":
          return PartialView("Partials/_LowStock", data.LowStockProducts);

        case "TopCustomers":
          return PartialView("Partials/_TopCustomers", data.TopCustomers);

        default:
          return Content("Không tìm thấy loại thống kê.");
      }
    }
  }
}
