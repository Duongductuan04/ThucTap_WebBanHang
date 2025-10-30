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
using Abp.UI;

namespace SimpleTaskApp.MobilePhones
{
    [AbpAuthorize]
    public class MobilePhoneAppService : SimpleTaskAppAppServiceBase, IMobilePhoneAppService
    {
        private readonly IRepository<MobilePhone, int> _mobilePhoneRepository;
        private readonly IRepository<MobilePhoneColor, int> _colorRepository;

        public MobilePhoneAppService(
            IRepository<MobilePhone, int> mobilePhoneRepository,
            IRepository<MobilePhoneColor, int> colorRepository)
        {
            _mobilePhoneRepository = mobilePhoneRepository;
            _colorRepository = colorRepository;
        }

        // ✅ CREATE
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
            await CurrentUnitOfWork.SaveChangesAsync(); // Lưu để có Id mới

            // ✅ Thêm danh sách màu
            if (input.Colors != null && input.Colors.Any())
            {
                foreach (var colorDto in input.Colors)
                {
                    var color = new MobilePhoneColor
                    {
                        MobilePhoneId = phone.Id,
                        ColorName = colorDto.ColorName,
                        ColorHex = colorDto.ColorHex,
                        ImageUrl = colorDto.ImageUrl
                    };
                    await _colorRepository.InsertAsync(color);
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return await GetAsync(new EntityDto<int>(phone.Id));
        }

        // ✅ UPDATE
        [AbpAuthorize]
        public async Task<MobilePhoneDto> UpdateAsync(UpdateMobilePhoneDto input)
        {
            var phone = await _mobilePhoneRepository.GetAsync(input.Id);
            if (phone == null)
                throw new KeyNotFoundException("Mobile phone not found");

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

            // ✅ Cập nhật lại danh sách màu
            var oldColors = await _colorRepository.GetAllListAsync(c => c.MobilePhoneId == phone.Id);
            foreach (var c in oldColors)
            {
                await _colorRepository.DeleteAsync(c);
            }

            if (input.Colors != null && input.Colors.Any())
            {
                foreach (var colorDto in input.Colors)
                {
                    var color = new MobilePhoneColor
                    {
                        MobilePhoneId = phone.Id,
                        ColorName = colorDto.ColorName,
                        ColorHex = colorDto.ColorHex,
                        ImageUrl = colorDto.ImageUrl
                    };
                    await _colorRepository.InsertAsync(color);
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return await GetAsync(new EntityDto<int>(phone.Id));
        }

        // ✅ DELETE
        [AbpAuthorize]
        public async Task DeleteAsync(EntityDto<int> input)
        {
            var phone = await _mobilePhoneRepository.GetAsync(input.Id);
            if (phone != null)
            {
                var colors = await _colorRepository.GetAllListAsync(c => c.MobilePhoneId == phone.Id);
                foreach (var color in colors)
                {
                    await _colorRepository.DeleteAsync(color);
                }

                await _mobilePhoneRepository.DeleteAsync(phone);
            }
        }

        // ✅ GET BY ID (có danh sách màu)
        public async Task<MobilePhoneDto> GetAsync(EntityDto<int> input)
        {
            var phone = await _mobilePhoneRepository.GetAll()
                .Include(p => p.Category)
                .Include(p => p.Colors)
                .FirstOrDefaultAsync(p => p.Id == input.Id);

            if (phone == null)
                throw new UserFriendlyException("Không tìm thấy sản phẩm");

            return MapToDto(phone);
        }




        // ✅ GET ALL + FILTER
        public async Task<PagedResultDto<MobilePhoneDto>> GetAllAsync(PagedMobilePhoneResultRequestDto input)
        {
            var query = _mobilePhoneRepository.GetAll()
                .Include(mp => mp.Category)
                .Include(mp => mp.Colors)
                .AsQueryable();

            query = query.Where(p => p.StockQuantity > 0);

            if (!string.IsNullOrEmpty(input.Keyword))
                query = query.Where(x => x.Name.Contains(input.Keyword) || x.Description.Contains(input.Keyword));

            if (input.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == input.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(input.Brand))
                query = query.Where(p => p.Brand == input.Brand);

            if (input.IsNew.HasValue && input.IsNew.Value)
                query = query.Where(p => p.IsNew == true);

            if (input.IsOnSale.HasValue && input.IsOnSale.Value)
            {
                var now = DateTime.Now;
                query = query.Where(p => p.IsOnSale == true &&
                                         p.SaleStart <= now &&
                                         (p.SaleEnd == null || p.SaleEnd >= now));
            }

            // Sort
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
                    query = query.OrderByDescending(p => p.DiscountPrice ?? p.Price);
                    break;
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

            return new PagedResultDto<MobilePhoneDto>(
                totalCount,
                items.Select(MapToDto).ToList()
            );
        }

        // ✅ GET BRANDS BY CATEGORY
        public async Task<List<string>> GetBrandsByCategoryAsync(int? categoryId)
        {
            var query = _mobilePhoneRepository.GetAll();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            return await query
                .Select(p => p.Brand)
                .Where(b => !string.IsNullOrEmpty(b))
                .Distinct()
                .ToListAsync();
        }
    public async Task<List<MobilePhoneColorDto>> GetColorsByMobilePhoneIdAsync(int mobilePhoneId)
    {
      // Lấy màu kèm MobilePhone
      var colors = await _colorRepository.GetAll()
          .Include(c => c.MobilePhone) // Include entity MobilePhone
          .Where(c => c.MobilePhoneId == mobilePhoneId)
          .OrderBy(c => c.Id)
          .ToListAsync();

      // Map thủ công
      var colorDtos = colors.Select(c => new MobilePhoneColorDto
      {
        Id = c.Id,
        MobilePhoneId = c.MobilePhoneId,
        ColorName = c.ColorName,
        ColorHex = c.ColorHex,
        ImageUrl = c.ImageUrl,
        MobilePhoneName = c.MobilePhone != null ? c.MobilePhone.Name : null
      }).ToList();

      return colorDtos;
    }
    // ✅ MAP ENTITY -> DTO
    private MobilePhoneDto MapToDto(MobilePhone phone)
        {
            if (phone == null) return null;

            var dto = new MobilePhoneDto
            {
                Id = phone.Id,
                Name = phone.Name,
                Description = phone.Description,
                Price = phone.Price,
                DiscountPrice = phone.DiscountPrice,
                StockQuantity = phone.StockQuantity,
                CategoryId = phone.CategoryId,
                CategoryName = phone.Category?.Name,
                Brand = phone.Brand,
                IsNew = phone.IsNew,
                IsOnSale = phone.IsOnSale,
                SaleStart = phone.SaleStart,
                SaleEnd = phone.SaleEnd,
                ImageUrl = phone.ImageUrl,
                CreationTime = phone.CreationTime,
                Colors = phone.Colors?.Select(c => new MobilePhoneColorDto
                {
                    Id = c.Id,
                    ColorName = c.ColorName,
                    ColorHex = c.ColorHex,
                    ImageUrl = c.ImageUrl
                }).ToList()
            };

            return dto;
        }
    }
}
