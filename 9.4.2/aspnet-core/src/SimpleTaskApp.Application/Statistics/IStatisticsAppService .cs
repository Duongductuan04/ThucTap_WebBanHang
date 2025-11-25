using Abp.Application.Services;
using SimpleTaskApp.Statistics.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTaskApp.Statistics
{
    public interface IStatisticsAppService : IApplicationService
    {
        Task<StatisticsDto> GetDashboardStatisticsAsync(StatisticsFilterDto filter);
    Task<List<LowStockProductVariantDto>> GetLowStockProductsAsync(int lowStockThreshold);
    Task<byte[]> ExportStatisticsToExcelAsync(StatisticsDto statistics);

  }
}
