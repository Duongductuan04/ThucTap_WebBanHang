using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using SimpleTaskApp.MobilePhones.Dto;

namespace SimpleTaskApp.MobilePhones
{
    public interface IMobilePhoneCategoryAppService : IApplicationService
    {
        Task<PagedResultDto<MobilePhoneCategoryDto>> GetAllAsync(PagedMobilePhoneCategoryResultRequestDto input);
        Task<string> GetNameAsync(int id); // hàm bất đồng bộ trả về kiêủ string

    }
}
