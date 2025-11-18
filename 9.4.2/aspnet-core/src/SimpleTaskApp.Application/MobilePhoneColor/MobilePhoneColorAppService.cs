using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.MobilePhones.Dto;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    [AbpAuthorize] // Chỉ cho phép người có quyền
    public class MobilePhoneColorAppService : ApplicationService, IMobilePhoneColorAppService
    {
        private readonly IRepository<MobilePhoneColor, int> _colorRepository;
        private readonly IRepository<MobilePhone, int> _phoneRepository;

        public MobilePhoneColorAppService(
            IRepository<MobilePhoneColor, int> colorRepository,
            IRepository<MobilePhone, int> phoneRepository)
        {
            _colorRepository = colorRepository;
            _phoneRepository = phoneRepository;
        }

        // ✅ Lấy danh sách phân trang + tìm kiếm
        public async Task<PagedResultDto<MobilePhoneColorDto>> GetAllAsync(PagedMobilePhoneColorResultRequestDto input)
        {
            var query = _colorRepository.GetAllIncluding(c => c.MobilePhone);

            // Áp dụng filter
            if (!string.IsNullOrEmpty(input.Keyword))
                query = query.Where(c => c.ColorName.Contains(input.Keyword));

            if (input.MobilePhoneId.HasValue)
                query = query.Where(c => c.MobilePhoneId == input.MobilePhoneId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.ColorName)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtoList = items.Select(MapToDto).ToList();

            return new PagedResultDto<MobilePhoneColorDto>(totalCount, dtoList);
        }

        // ✅ Lấy chi tiết 1 màu
        public async Task<MobilePhoneColorDto> GetAsync(EntityDto<int> input)
        {
            var color = await _colorRepository.GetAllIncluding(c => c.MobilePhone)
                .FirstOrDefaultAsync(c => c.Id == input.Id);

            return MapToDto(color);
        }

  

        // 🧠 Map thủ công từ Entity -> DTO
        private MobilePhoneColorDto MapToDto(MobilePhoneColor color)
        {
            if (color == null) return null;

            return new MobilePhoneColorDto
            {
                Id = color.Id,
                MobilePhoneId = color.MobilePhoneId,
                MobilePhoneName = color.MobilePhone?.Name,
                ColorName = color.ColorName,
                ColorHex = color.ColorHex,
                ImageUrl = color.ImageUrl
            };
        }
    }
}
