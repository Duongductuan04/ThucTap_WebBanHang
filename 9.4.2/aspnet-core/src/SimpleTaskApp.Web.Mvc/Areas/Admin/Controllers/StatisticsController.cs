using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.Controllers;
using SimpleTaskApp.Statistics;
using SimpleTaskApp.Statistics.Dto;
using System.Threading.Tasks;

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
      // Nếu chưa chọn năm → mặc định năm hiện tại
      if (filter.Year == null)
      {
        filter.Year = System.DateTime.Now.Year;
      }

      var stats = await _statisticsAppService.GetDashboardStatisticsAsync(filter);

      ViewBag.Filter = filter;

      // Trả về 4 box + biểu đồ doanh thu
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
