using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using SimpleTaskApp.MobilePhones.Dto;

namespace SimpleTaskApp.MobilePhones
{
  public interface IMobilePhoneCategoryAppService : IApplicationService
  {
    // Lấy danh sách phân trang
    Task<PagedResultDto<MobilePhoneCategoryDto>> GetAllAsync(PagedMobilePhoneCategoryResultRequestDto input);

    // Lấy tên category theo Id
    Task<string> GetNameAsync(int id);

    // Lấy 1 category theo Id
    Task<MobilePhoneCategoryDto> GetAsync(EntityDto<int> input);

    // Tạo mới category
    Task<MobilePhoneCategoryDto> CreateAsync(CreateMobilePhoneCategoryDto input);

    // Cập nhật category
    Task<MobilePhoneCategoryDto> UpdateAsync(UpdateMobilePhoneCategoryDto input);

    // Xóa category
    Task DeleteAsync(EntityDto<int> input);
  }
}
