using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using SimpleTaskApp.MobilePhones.Dto;

namespace SimpleTaskApp.MobilePhones
{
    public interface IOrderDetailAppService : IApplicationService
    {
        Task<PagedResultDto<OrderDetailDto>> GetAllAsync(PagedOrderDetailResultRequestDto input);
    }
}
