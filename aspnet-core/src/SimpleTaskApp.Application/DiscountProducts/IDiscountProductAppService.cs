using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SimpleTaskApp.DiscountProducts.Dto;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    public interface IDiscountProductAppService : IApplicationService
    {
   

        // Lấy DiscountProduct theo Id

        // Phân trang
        Task<PagedResultDto<DiscountProductDto>> GetAllAsync(PagedDiscountProductResultRequestDto input);
    }
}
