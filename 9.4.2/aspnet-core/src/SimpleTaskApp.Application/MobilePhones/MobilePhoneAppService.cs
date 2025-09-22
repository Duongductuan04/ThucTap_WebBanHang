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
                StockQuantity = input.StockQuantity,
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
            phone.StockQuantity = input.StockQuantity;
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
        public async Task<PagedResultDto<MobilePhoneDto>> GetAllAsync(PagedMobilePhoneResultRequestDto input)
        {
            var query = _mobilePhoneRepository.GetAll()
       .Include(mp => mp.Category)
       .AsQueryable();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x => x.Name.Contains(input.Keyword) ||
                                        x.Description.Contains(input.Keyword));
            }

            if (input.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == input.CategoryId.Value);
            }
            // Lọc theo hàng mớix
            if (input.IsNew.HasValue)
            {
                query = query.Where(x => x.IsNew == input.IsNew.Value);
            }

            // Lọc theo khuyến mãi (sản phẩm đang trong thời gian khuyến mãi)
            if (input.IsOnSale.HasValue && input.IsOnSale.Value) // Đổi thành IsOnSale
            {
                var now = DateTime.Now;
                query = query.Where(x => x.IsOnSale &&
                                        x.SaleStart <= now &&
                                        (x.SaleEnd == null || x.SaleEnd >= now));
            }
            if (input.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= input.MinPrice.Value);
            }

            if (input.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= input.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Brand))
            {
                query = query.Where(p => p.Brand == input.Brand);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

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
