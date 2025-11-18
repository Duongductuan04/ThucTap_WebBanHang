using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SimpleTaskApp.MobilePhones.Dto;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    // Interface cho Discount AppService
    public interface IDiscountAppService : IApplicationService
    {
        // Tạo voucher mới
        Task<DiscountDto> CreateAsync(CreateDiscountDto input);

        // Cập nhật voucher
        Task<DiscountDto> UpdateAsync(UpdateDiscountDto input);

        // Xóa voucher
        Task DeleteAsync(EntityDto<int> input);

        // Lấy voucher theo Id
        Task<DiscountDto> GetAsync(EntityDto<int> input);

        // Phân trang voucher
        Task<PagedResultDto<DiscountDto>> GetAllAsync(PagedDiscountResultRequestDto input);
    }
}
