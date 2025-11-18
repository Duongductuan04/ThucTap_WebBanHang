using Abp.Application.Services;
using SimpleTaskApp.Statistics.Dto;
using System.Threading.Tasks;

namespace SimpleTaskApp.Statistics
{
    public interface IStatisticsAppService : IApplicationService
    {
        Task<StatisticsDto> GetDashboardStatisticsAsync(StatisticsFilterDto filter);
    }
}
