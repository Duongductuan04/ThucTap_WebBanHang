using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization; // THÊM NAMESPACE NÀY
using Abp.Domain.Repositories;
using SimpleTaskApp.Authorization; // THÊM NAMESPACE NÀY
using SimpleTaskApp.MobilePhones.Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    [AbpAuthorize] // THÊM AUTHORIZATION Ở ĐÂY
    public class MobilePhoneCategoryAppService : ApplicationService, IMobilePhoneCategoryAppService
    {
        private readonly IRepository<MobilePhoneCategory, int> _categoryRepository;

        public MobilePhoneCategoryAppService(IRepository<MobilePhoneCategory, int> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // Lấy danh sách phân trang MobilePhoneCategory
        public async Task<PagedResultDto<MobilePhoneCategoryDto>> GetAllAsync(PagedMobilePhoneCategoryResultRequestDto input)
        {
            var query = _categoryRepository.GetAll();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Name)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtoList = items.Select(MapToDto).ToList();

            return new PagedResultDto<MobilePhoneCategoryDto>(totalCount, dtoList);
        }
        public async Task<string> GetNameAsync(int id)
        {
            var category = await _categoryRepository.GetAsync(id); // Hoặc gọi GetAsync(id) từ service
            return category?.Name;
        }
        // Map tay từ Entity -> DTO
        private MobilePhoneCategoryDto MapToDto(MobilePhoneCategory category)
        {
            if (category == null) return null;

            return new MobilePhoneCategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}