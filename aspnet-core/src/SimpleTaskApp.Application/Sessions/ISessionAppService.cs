using System.Threading.Tasks;
using Abp.Application.Services;
using SimpleTaskApp.Sessions.Dto;

namespace SimpleTaskApp.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
