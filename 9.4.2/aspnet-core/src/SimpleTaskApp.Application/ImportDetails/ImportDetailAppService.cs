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
    [AbpAuthorize]
    public class ImportDetailAppService : ApplicationService, IImportDetailAppService
    {
        private readonly IRepository<ImportDetail, int> _importDetailRepository;
        private readonly IRepository<Import, int> _importRepository;
        private readonly IRepository<MobilePhone, int> _mobilePhoneRepository;
        private readonly IRepository<MobilePhoneColor, int> _colorRepository;

        public ImportDetailAppService(
            IRepository<ImportDetail, int> importDetailRepository,
            IRepository<Import, int> importRepository,
            IRepository<MobilePhone, int> mobilePhoneRepository,
            IRepository<MobilePhoneColor, int> colorRepository)

        {
            _importDetailRepository = importDetailRepository;
            _importRepository = importRepository;
            _mobilePhoneRepository = mobilePhoneRepository;
            _colorRepository = colorRepository;

        }

        // ==================== Lấy danh sách (phân trang + tìm kiếm) ====================
        public async Task<PagedResultDto<ImportDetailDto>> GetAllAsync(PagedImportDetailResultRequestDto input)
        {
            var query = _importDetailRepository.GetAllIncluding(d => d.Import);

        

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(d => d.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtoList = items.Select(MapToDto).ToList();

            return new PagedResultDto<ImportDetailDto>(totalCount, dtoList);
        }

        // ==================== Lấy 1 chi tiết theo ID ====================
        public async Task<ImportDetailDto> GetAsync(EntityDto<int> input)
        {
            var detail = await _importDetailRepository.GetAsync(input.Id);
            return MapToDto(detail);
        }

        // ==================== Map Entity -> DTO ====================
        private ImportDetailDto MapToDto(ImportDetail detail)
        {
            if (detail == null) return null;

            return new ImportDetailDto
            {
                Id = detail.Id,
                ImportId = detail.ImportId,
                MobilePhoneId = detail.MobilePhoneId,
                Quantity = detail.Quantity,
                ImportPrice = detail.ImportPrice,
            };
        }
    }
}
