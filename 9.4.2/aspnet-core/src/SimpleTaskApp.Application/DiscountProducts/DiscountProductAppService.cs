using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.DiscountProducts.Dto;
using SimpleTaskApp.MobilePhones;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.DiscountProducts
{
    [AbpAuthorize]
    public class DiscountProductAppService : ApplicationService, IDiscountProductAppService
    {
        private readonly IRepository<DiscountProduct, int> _discountProductRepository;

        public DiscountProductAppService(IRepository<DiscountProduct, int> discountProductRepository)
        {
            _discountProductRepository = discountProductRepository;
        }

        // Lấy danh sách phân trang
        public async Task<PagedResultDto<DiscountProductDto>> GetAllAsync(PagedDiscountProductResultRequestDto input)
        {
            var query = _discountProductRepository.GetAllIncluding(dp => dp.MobilePhone);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(dp => dp.DiscountId)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtoList = items.Select(MapToDiscountProductDto).ToList();

            return new PagedResultDto<DiscountProductDto>(totalCount, dtoList);
        }

        // Map entity -> DTO
        private DiscountProductDto MapToDiscountProductDto(DiscountProduct dp)
        {
            if (dp == null) return null;

            return new DiscountProductDto
            {
                Id = dp.Id,
                DiscountId = dp.DiscountId,
                MobilePhoneId = dp.MobilePhoneId,
                MobilePhoneName = dp.MobilePhone?.Name
            };
        }
    }
}
