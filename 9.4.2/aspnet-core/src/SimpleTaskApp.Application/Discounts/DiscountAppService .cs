using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.MobilePhones.Dto;
using SimpleTaskApp.DiscountCategorys.Dto;
using SimpleTaskApp.DiscountProducts.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    [AbpAuthorize]
    public class DiscountAppService : ApplicationService, IDiscountAppService
    {
        private readonly IRepository<Discount, int> _discountRepository;
        private readonly IRepository<DiscountCategory, int> _discountCategoryRepository;
        private readonly IRepository<DiscountProduct, int> _discountProductRepository;

        public DiscountAppService(
            IRepository<Discount, int> discountRepository,
            IRepository<DiscountCategory, int> discountCategoryRepository,
            IRepository<DiscountProduct, int> discountProductRepository)
        {
            _discountRepository = discountRepository;
            _discountCategoryRepository = discountCategoryRepository;
            _discountProductRepository = discountProductRepository;
        }

        // ================== TẠO VOUCHER ==================
        public async Task<DiscountDto> CreateAsync(CreateDiscountDto input)
        {
            var discount = new Discount
            {
                Name = input.Name,
                Code = input.Code,
                Percentage = input.Percentage,
                Amount = input.Amount,
                MinOrderValue = input.MinOrderValue,
                ApplyType = input.ApplyType,
                MaxUsage = input.MaxUsage,
                CurrentUsage = 0,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                IsActive = input.IsActive
            };

            await _discountRepository.InsertAsync(discount);
            await CurrentUnitOfWork.SaveChangesAsync(); // Sinh Id

            await CreateOrUpdateIntermediateTables(discount.Id, input);

            return await GetAsync(new EntityDto<int> { Id = discount.Id });
        }

        // ================== CẬP NHẬT VOUCHER ==================
        public async Task<DiscountDto> UpdateAsync(UpdateDiscountDto input)
        {
            var discount = await _discountRepository.GetAsync(input.Id);
            if (discount == null)
                throw new UserFriendlyException($"Discount not found! Id = {input.Id}");

            discount.Name = input.Name;
            discount.Code = input.Code;
            discount.Percentage = input.Percentage;
            discount.Amount = input.Amount;
            discount.MinOrderValue = input.MinOrderValue;
            discount.ApplyType = input.ApplyType;
            discount.MaxUsage = input.MaxUsage;
            discount.StartDate = input.StartDate;
            discount.EndDate = input.EndDate;
            discount.IsActive = input.IsActive;

            await _discountRepository.UpdateAsync(discount);

            // Xóa bảng trung gian cũ
            var oldCategories = _discountCategoryRepository.GetAll().Where(dc => dc.DiscountId == discount.Id);
            foreach (var c in oldCategories) await _discountCategoryRepository.DeleteAsync(c.Id);

            var oldProducts = _discountProductRepository.GetAll().Where(dp => dp.DiscountId == discount.Id);
            foreach (var p in oldProducts) await _discountProductRepository.DeleteAsync(p.Id);

            // Tạo bảng trung gian mới
            await CreateOrUpdateIntermediateTables(discount.Id, input);

            return await GetAsync(new EntityDto<int> { Id = discount.Id });
        }

        // ================== LẤY VOUCHER THEO ID ==================
        public async Task<DiscountDto> GetAsync(EntityDto<int> input)
        {
            var discount = await _discountRepository.GetAll().FirstOrDefaultAsync(d => d.Id == input.Id);
            if (discount == null)
                throw new UserFriendlyException($"Discount not found! Id = {input.Id}");

            return await MapToDiscountDto(discount);
        }

        // ================== PHÂN TRANG ==================
        public async Task<PagedResultDto<DiscountDto>> GetAllAsync(PagedDiscountResultRequestDto input)
        {
            var query = _discountRepository.GetAll();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(d => d.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtoList = new List<DiscountDto>();
            foreach (var discount in items)
            {
                dtoList.Add(await MapToDiscountDto(discount));
            }

            return new PagedResultDto<DiscountDto>(totalCount, dtoList);
        }

        // ================== XÓA VOUCHER ==================
        public async Task DeleteAsync(EntityDto<int> input)
        {
            if (input == null) return;

            var categories = _discountCategoryRepository.GetAll().Where(dc => dc.DiscountId == input.Id);
            foreach (var c in categories)
                await _discountCategoryRepository.DeleteAsync(c.Id);

            var products = _discountProductRepository.GetAll().Where(dp => dp.DiscountId == input.Id);
            foreach (var p in products)
                await _discountProductRepository.DeleteAsync(p.Id);

            await _discountRepository.DeleteAsync(input.Id);
        }

        // ================== MAP THỦ CÔNG ==================
        private async Task<DiscountDto> MapToDiscountDto(Discount discount)
        {
            if (discount == null) return null;

            var categories = await _discountCategoryRepository.GetAll()
                .Where(dc => dc.DiscountId == discount.Id)
                .Include(dc => dc.Category)
                .ToListAsync();

            var products = await _discountProductRepository.GetAll()
                .Where(dp => dp.DiscountId == discount.Id)
                .Include(dp => dp.MobilePhone)
                .ToListAsync();

            return new DiscountDto
            {
                Id = discount.Id,
                Name = discount.Name,
                Code = discount.Code,
                Percentage = discount.Percentage,
                Amount = discount.Amount,
                MinOrderValue = discount.MinOrderValue,
                ApplyType = discount.ApplyType,
                MaxUsage = discount.MaxUsage,
                CurrentUsage = discount.CurrentUsage,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = discount.IsActive,
                CreationTime = discount.CreationTime,
                Categories = discount.ApplyType == 1 ? categories.Select(c => new DiscountCategoryDto
                {
                    Id = c.Id,
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category?.Name
                }).ToList() : new List<DiscountCategoryDto>(),
                Products = discount.ApplyType == 2 ? products.Select(p => new DiscountProductDto
                {
                    Id = p.Id,
                    MobilePhoneId = p.MobilePhoneId,
                    MobilePhoneName = p.MobilePhone?.Name
                }).ToList() : new List<DiscountProductDto>()
            };
        }

        // ================== PRIVATE: TẠO BẢNG TRUNG GIAN ==================
        private async Task CreateOrUpdateIntermediateTables(int discountId, CreateDiscountDto input)
        {
            if (input.ApplyType == 1 && input.Categories.Any())
            {
                foreach (var c in input.Categories)
                {
                    await _discountCategoryRepository.InsertAsync(new DiscountCategory
                    {
                        DiscountId = discountId,
                        CategoryId = c.CategoryId
                    });
                }
            }

            if (input.ApplyType == 2 && input.Products.Any())
            {
                foreach (var p in input.Products)
                {
                    await _discountProductRepository.InsertAsync(new DiscountProduct
                    {
                        DiscountId = discountId,
                        MobilePhoneId = p.MobilePhoneId
                    });
                }
            }
        }

        // ================== CHECK VOUCHER KHI ORDER ==================
        public async Task<Discount> ApplyDiscountAsync(int discountId)
        {
            var discount = await _discountRepository.GetAsync(discountId);
            if (discount == null)
                throw new UserFriendlyException("Voucher không tồn tại.");

            var now = System.DateTime.Now;

            if ((discount.StartDate != null && now < discount.StartDate) ||
                (discount.EndDate != null && now > discount.EndDate))
                throw new UserFriendlyException("Voucher chưa có hiệu lực hoặc đã hết hạn.");

            if (discount.MaxUsage != 0 && discount.CurrentUsage >= discount.MaxUsage)
                throw new UserFriendlyException("Voucher đã hết lượt sử dụng.");

            // Tăng CurrentUsage
            discount.CurrentUsage += 1;
            await _discountRepository.UpdateAsync(discount);

            return discount;
        }
    }
}
