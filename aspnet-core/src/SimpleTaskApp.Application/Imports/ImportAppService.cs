using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.MobilePhones.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    [AbpAuthorize]
    public class ImportAppService : ApplicationService, IImportAppService
    {
        private readonly IRepository<Import, int> _importRepository;
        private readonly IRepository<ImportDetail, int> _importDetailRepository;
        private readonly IRepository<MobilePhone, int> _mobilePhoneRepository;

        public ImportAppService(
            IRepository<Import, int> importRepository,
            IRepository<ImportDetail, int> importDetailRepository,
            IRepository<MobilePhone, int> mobilePhoneRepository)
        {
            _importRepository = importRepository;
            _importDetailRepository = importDetailRepository;
            _mobilePhoneRepository = mobilePhoneRepository;
        }

        // ================== TẠO PHIẾU NHẬP ==================
        public async Task<ImportDto> CreateAsync(CreateImportDto input)
        {
            // Sinh mã phiếu nhập tự động
            var importCode = $"PN{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";

            var import = new Import
            {
                ImportCode = importCode,
                SupplierName = input.SupplierName,
                Note = input.Note,
                KeeperName = input.KeeperName,
                KeeperPhone = input.KeeperPhone
            };

            await _importRepository.InsertAsync(import);
            await CurrentUnitOfWork.SaveChangesAsync(); // để sinh Id

            // Thêm chi tiết phiếu nhập
            await CreateOrUpdateDetails(import.Id, input.ImportDetails);

            return await GetAsync(new EntityDto<int> { Id = import.Id });
        }

        // ================== CẬP NHẬT PHIẾU NHẬP ==================
        public async Task<ImportDto> UpdateAsync(UpdateImportDto input)
        {
            var import = await _importRepository.GetAll()
                .Include(i => i.ImportDetails)
                .ThenInclude(d => d.MobilePhone)
                .FirstOrDefaultAsync(i => i.Id == input.Id);

            if (import == null)
                throw new UserFriendlyException($"Không tìm thấy phiếu nhập Id = {input.Id}");

            import.SupplierName = input.SupplierName;
            import.Note = input.Note;
            import.KeeperName = input.KeeperName;
            import.KeeperPhone = input.KeeperPhone;

            await _importRepository.UpdateAsync(import);

            // Xử lý tồn kho
            var oldDetails = import.ImportDetails.ToList();

            // Trừ tồn kho cũ
            foreach (var d in oldDetails)
            {
                if (d.MobilePhone != null)
                {
                    d.MobilePhone.StockQuantity = Math.Max(d.MobilePhone.StockQuantity - d.Quantity, 0);
                    await _mobilePhoneRepository.UpdateAsync(d.MobilePhone);
                }
                await _importDetailRepository.DeleteAsync(d.Id);
            }

            // Thêm chi tiết mới và cộng tồn kho
            await CreateOrUpdateDetails(import.Id, input.ImportDetails);

            return await GetAsync(new EntityDto<int> { Id = import.Id });
        }


        // ================== LẤY PHIẾU NHẬP THEO ID ==================
        public async Task<ImportDto> GetAsync(EntityDto<int> input)
        {
            var import = await _importRepository.GetAll()
                .Include(i => i.ImportDetails)
                .ThenInclude(d => d.MobilePhone)
                .FirstOrDefaultAsync(i => i.Id == input.Id);

            if (import == null)
                throw new UserFriendlyException($"Không tìm thấy phiếu nhập Id = {input.Id}");

            return MapToDto(import);
        }

        // ================== PHÂN TRANG & TÌM KIẾM ==================
        public async Task<PagedResultDto<ImportDto>> GetAllAsync(PagedImportResultRequestDto input)
        {
            var query = _importRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(input.Keyword))
                query = query.Where(x => x.ImportCode.Contains(input.Keyword) || x.SupplierName.Contains(input.Keyword));

            var totalCount = await query.CountAsync();

            var imports = await query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Include(x => x.ImportDetails)
                .ThenInclude(d => d.MobilePhone)
                .ToListAsync();

            var dtoList = imports.Select(MapToDto).ToList();

            return new PagedResultDto<ImportDto>(totalCount, dtoList);
        }

        // ================== XÓA PHIẾU NHẬP ==================
        public async Task DeleteAsync(EntityDto<int> input)
        {
            // Lấy tất cả chi tiết phiếu nhập kèm sản phẩm
            var details = await _importDetailRepository.GetAll()
                .Include(d => d.MobilePhone)
                .Where(d => d.ImportId == input.Id)
                .ToListAsync();

            foreach (var d in details)
            {
                // Trừ tồn kho, đảm bảo không âm
                if (d.MobilePhone != null)
                {
                    d.MobilePhone.StockQuantity = Math.Max(d.MobilePhone.StockQuantity - d.Quantity, 0);
                    await _mobilePhoneRepository.UpdateAsync(d.MobilePhone);
                }

                // Xóa chi tiết phiếu nhập
                await _importDetailRepository.DeleteAsync(d.Id);
            }

            // Xóa phiếu nhập
            await _importRepository.DeleteAsync(input.Id);
        }


        // ================== HÀM RIÊNG: TẠO CHI TIẾT PHIẾU NHẬP ==================
        private async Task CreateOrUpdateDetails(int importId, List<CreateImportDetailDto> details)
        {
            if (details == null || !details.Any()) return;

            foreach (var d in details)
            {
                var importDetail = new ImportDetail
                {
                    ImportId = importId,
                    MobilePhoneId = d.MobilePhoneId,
                    Quantity = d.Quantity,
                    ImportPrice = d.ImportPrice
                };

                await _importDetailRepository.InsertAsync(importDetail);

                // Cộng tồn kho cho sản phẩm
                var phone = await _mobilePhoneRepository.GetAsync(d.MobilePhoneId);
                phone.StockQuantity += d.Quantity;
                await _mobilePhoneRepository.UpdateAsync(phone);
            }
        }

        // ================== MAP ENTITY -> DTO ==================
        private ImportDto MapToDto(Import import)
        {
            return new ImportDto
            {
                Id = import.Id,
                ImportCode = import.ImportCode,
                SupplierName = import.SupplierName,
                Note = import.Note,
                KeeperName = import.KeeperName,
                KeeperPhone = import.KeeperPhone,
                CreationTime = import.CreationTime,
                ImportDetails = import.ImportDetails?.Select(d => new ImportDetailDto
                {
                    Id = d.Id,
                    MobilePhoneId = d.MobilePhoneId,
                    MobilePhoneName = d.MobilePhone?.Name,
                    Quantity = d.Quantity,
                    ImportPrice = d.ImportPrice
                }).ToList()
            };
        }
    }
}