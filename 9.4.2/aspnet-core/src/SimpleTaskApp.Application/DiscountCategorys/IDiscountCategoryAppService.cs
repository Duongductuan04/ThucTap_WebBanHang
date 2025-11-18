using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SimpleTaskApp.DiscountCategorys.Dto;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    public interface IDiscountCategoryAppService : IApplicationService
    {
        // Tạo mới DiscountCategory
     

        // Lấy DiscountCategory theo Id
     

        // Phân trang
        Task<PagedResultDto<DiscountCategoryDto>> GetAllAsync(PagedDiscountCategoryResultRequestDto input);
    }
}
