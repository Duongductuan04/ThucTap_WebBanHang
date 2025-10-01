using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.DiscountCategorys.Dto;
using SimpleTaskApp.MobilePhones;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.DiscountCategorys
{
    [AbpAuthorize]
    public class DiscountCategoryAppService : ApplicationService, IDiscountCategoryAppService
    {
        private readonly IRepository<DiscountCategory, int> _discountCategoryRepository;

        public DiscountCategoryAppService(IRepository<DiscountCategory, int> discountCategoryRepository)
        {
            _discountCategoryRepository = discountCategoryRepository;
        }

        // Lấy danh sách phân trang
        public async Task<PagedResultDto<DiscountCategoryDto>> GetAllAsync(PagedDiscountCategoryResultRequestDto input)
        {
            var query = _discountCategoryRepository.GetAllIncluding(dc => dc.Category);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(dc => dc.DiscountId)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtoList = items.Select(MapToDiscountCategoryDto).ToList();

            return new PagedResultDto<DiscountCategoryDto>(totalCount, dtoList);
        }

        // Map entity -> DTO
        private DiscountCategoryDto MapToDiscountCategoryDto(DiscountCategory dc)
        {
            if (dc == null) return null;

            return new DiscountCategoryDto
            {
                Id = dc.Id,
                DiscountId = dc.DiscountId,
                CategoryId = dc.CategoryId,
                CategoryName = dc.Category?.Name
            };
        }
    }
}
