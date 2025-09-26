using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using SimpleTaskApp.MobilePhones.Dto;

namespace SimpleTaskApp.MobilePhones
{
    public interface IMobilePhoneAppService : IApplicationService
    {
        Task<MobilePhoneDto> CreateAsync(CreateMobilePhoneDto input);
        Task<MobilePhoneDto> UpdateAsync(UpdateMobilePhoneDto input);
        Task DeleteAsync(EntityDto<int> input);
        Task<MobilePhoneDto> GetAsync(EntityDto<int> input);
        Task<PagedResultDto<MobilePhoneDto>> GetAllAsync(PagedMobilePhoneResultRequestDto input);
        Task<List<string>> GetBrandsByCategoryAsync(int? categoryId);


    }
}
