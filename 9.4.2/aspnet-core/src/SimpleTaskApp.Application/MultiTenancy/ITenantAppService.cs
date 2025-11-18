using Abp.Application.Services;
using SimpleTaskApp.MultiTenancy.Dto;

namespace SimpleTaskApp.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

