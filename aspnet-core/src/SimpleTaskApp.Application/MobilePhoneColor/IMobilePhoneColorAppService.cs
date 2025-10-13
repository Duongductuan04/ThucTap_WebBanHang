using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SimpleTaskApp.MobilePhones.Dto;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    public interface IMobilePhoneColorAppService : IApplicationService
    {
        // ✅ Lấy danh sách màu có phân trang + tìm kiếm
        Task<PagedResultDto<MobilePhoneColorDto>> GetAllAsync(PagedMobilePhoneColorResultRequestDto input);

        // ✅ Lấy chi tiết 1 màu theo Id
        Task<MobilePhoneColorDto> GetAsync(EntityDto<int> input);


    }
}
