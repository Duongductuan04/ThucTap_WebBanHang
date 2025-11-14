using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using SimpleTaskApp.MobilePhones.Dto;
using System;
using System.IO;

namespace SimpleTaskApp.MobilePhones
{
    public interface IMobilePhoneAppService : IApplicationService
    {
        Task<MobilePhoneDto> CreateAsync(CreateMobilePhoneDto input);
        Task<MobilePhoneDto> UpdateAsync(UpdateMobilePhoneDto input);
        Task DeleteAsync(EntityDto<int> input);
        Task<MobilePhoneDto> GetAsync(EntityDto<int> input);
        Task<PagedResultDto<MobilePhoneDto>> GetAllAsync(PagedMobilePhoneResultRequestDto input);
    Task<PagedResultDto<MobilePhoneDto>> GetAllByUserAsync(PagedMobilePhoneResultRequestDto input);

    Task<List<string>> GetBrandsByCategoryAsync(int? categoryId);
         Task<List<MobilePhoneColorDto>> GetColorsByMobilePhoneIdAsync(int mobilePhoneId);

         Task<byte[]> ExportToExcelByFilterAsync(int? categoryId, DateTime? from, DateTime? to);
         Task<int> ImportFromExcelAsync(Stream fileStream);

  }
}
