using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    public interface ICartAppService : IApplicationService
    {
        Task<CartDto> AddToCartAsync(CreateCartDto input);
        Task<CartDto> UpdateAsync(UpdateCartDto input);
        Task DeleteAsync(EntityDto<int> input);
        Task<CartDto> GetAsync(EntityDto<int> input);
        Task<PagedResultDto<CartDto>> GetAllAsync(PagedCartResultRequestDto input);
        Task<decimal> GetCartTotalAsync(long userId);
        Task<List<CartDto>> GetMyCartAsync();
        Task ClearMyCartAsync();
    }
}
