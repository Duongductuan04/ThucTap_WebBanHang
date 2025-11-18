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
            if (filter.Year == null)
            {
                filter.Year = System.DateTime.Now.Year;
            }
            var stats = await _statisticsAppService.GetDashboardStatisticsAsync(filter);
            ViewBag.Filter = filter;
            return View(stats);
        }
    }
}
