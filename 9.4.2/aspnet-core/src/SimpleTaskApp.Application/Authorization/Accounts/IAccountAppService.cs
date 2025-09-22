using System.Threading.Tasks;
using Abp.Application.Services;
using SimpleTaskApp.Authorization.Accounts.Dto;

namespace SimpleTaskApp.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
