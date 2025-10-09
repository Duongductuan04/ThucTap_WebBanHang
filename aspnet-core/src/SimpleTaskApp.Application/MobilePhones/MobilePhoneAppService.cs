using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.MobilePhones.Dto;
using System;

namespace SimpleTaskApp.MobilePhones
{
    [AbpAuthorize] // Chỉ user/admin có quyền xem
    public class MobilePhoneAppService : SimpleTaskAppAppServiceBase, IMobilePhoneAppService
    {
        private readonly IRepository<MobilePhone, int> _mobilePhoneRepository;

        public MobilePhoneAppService(IRepository<MobilePhone, int> mobilePhoneRepository)
        {
            _mobilePhoneRepository = mobilePhoneRepository;
        }

        // CREATE
        [AbpAuthorize(PermissionNames.Pages_MobilePhone_Create)]
        public async Task<MobilePhoneDto> CreateAsync(CreateMobilePhoneDto input)
        {
            var phone = new MobilePhone
            {
                Name = input.Name,
                Description = input.Description,
                Price = input.Price,
                DiscountPrice = input.DiscountPrice,
                StockQuantity = 0,
                CategoryId = input.CategoryId,
                Brand = input.Brand,
                IsNew = input.IsNew,
                IsOnSale = input.IsOnSale,
                SaleStart = input.SaleStart,
                SaleEnd = input.SaleEnd,
                ImageUrl = input.ImageUrl
            };

            await _mobilePhoneRepository.InsertAsync(phone);

            return MapToDto(phone);
        }

        // UPDATE
      [AbpAuthorize]
        public async Task<MobilePhoneDto> UpdateAsync(UpdateMobilePhoneDto input)
        {
            var phone = await _mobilePhoneRepository.GetAsync(input.Id);
            if (phone == null) throw new KeyNotFoundException("Mobile phone not found");

            phone.Name = input.Name;
            phone.Description = input.Description;
            phone.Price = input.Price;
            phone.DiscountPrice = input.DiscountPrice;
            phone.CategoryId = input.CategoryId;
            phone.Brand = input.Brand;
            phone.IsNew = input.IsNew;
            phone.IsOnSale = input.IsOnSale;
            phone.SaleStart = input.SaleStart;
            phone.SaleEnd = input.SaleEnd;
            phone.ImageUrl = input.ImageUrl;

            await _mobilePhoneRepository.UpdateAsync(phone);

            return MapToDto(phone);
        }

        // DELETE
        [AbpAuthorize]
        public async Task DeleteAsync(EntityDto<int> input)
        {
            var phone = await _mobilePhoneRepository.GetAsync(input.Id);
            if (phone != null)
            {
                await _mobilePhoneRepository.DeleteAsync(phone);
            }
        }

        // GET BY ID
        public async Task<MobilePhoneDto> GetAsync(EntityDto<int> input)
        {
            var phone = await _mobilePhoneRepository.GetAll()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == input.Id);

            return MapToDto(phone);
        }

        // GET ALL WITH PAGINATION + SEARCH
        // GET ALL WITH PAGINATION + SEARCH
        public async Task<PagedResultDto<MobilePhoneDto>> GetAllAsync(PagedMobilePhoneResultRequestDto input)
        {
            var query = _mobilePhoneRepository.GetAll()
                .Include(mp => mp.Category)
                .AsQueryable();
            query = query.Where(p => p.StockQuantity > 0);

            // 1. Lọc theo từ khóa
            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x => x.Name.Contains(input.Keyword) ||
                                         x.Description.Contains(input.Keyword));
            }

            // 2. Lọc theo danh mục
            if (input.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == input.CategoryId.Value);
            }

            // 3. Lọc theo brand
            if (!string.IsNullOrWhiteSpace(input.Brand))
            {
                query = query.Where(p => p.Brand == input.Brand);
            }

            // ⭐ 4. Lọc theo IsNew
            if (input.IsNew.HasValue && input.IsNew.Value)
            {
                query = query.Where(p => p.IsNew == true);
            }

            // ⭐ 5. Lọc theo IsOnSale
            if (input.IsOnSale.HasValue && input.IsOnSale.Value)
            {
                var now = DateTime.Now;
                query = query.Where(p => p.IsOnSale == true &&
                                         p.SaleStart <= now &&
                                         (p.SaleEnd == null || p.SaleEnd >= now));
            }

            // 6. Tổng số bản ghi sau filter
            var totalCount = await query.CountAsync();

            // 7. Sắp xếp
            switch (input.Sort)
            {
                case "priceAsc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "priceDesc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "new":
                    query = query.OrderByDescending(p => p.CreationTime);
                    break;
                case "sale":
                    query = query.OrderByDescending(p => p.DiscountPrice.HasValue ? p.DiscountPrice.Value : p.Price);
                    break;
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }

            // 8. Phân trang
            var items = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            // 9. Trả về DTO
            return new PagedResultDto<MobilePhoneDto>(
                totalCount,
                items.Select(p => MapToDto(p)).ToList()
            );
        }



        public async Task<List<string>> GetBrandsByCategoryAsync(int? categoryId)
        {
            var query = _mobilePhoneRepository.GetAll();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return await query
                .Select(p => p.Brand)
                .Where(b => !string.IsNullOrEmpty(b))
                .Distinct()
                .ToListAsync();
        }



        // HELPER: MAP TAY
        private MobilePhoneDto MapToDto(MobilePhone phone)
        {
            if (phone == null) return null;

            return new MobilePhoneDto
            {
                Id = phone.Id,
                Name = phone.Name,
                Description = phone.Description,
                Price = phone.Price,
                DiscountPrice = phone.DiscountPrice,
                StockQuantity = phone.StockQuantity,
                CategoryId = phone.CategoryId,
                CategoryName = phone.Category != null ? phone.Category.Name : null,
                Brand = phone.Brand,
                IsNew = phone.IsNew,
                IsOnSale = phone.IsOnSale,
                SaleStart = phone.SaleStart,
                SaleEnd = phone.SaleEnd,
                ImageUrl = phone.ImageUrl,
                CreationTime = phone.CreationTime
            };
        }
    }
}
