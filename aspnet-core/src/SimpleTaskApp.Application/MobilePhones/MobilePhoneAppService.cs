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
using MiniExcelLibs;
using System.IO;
using System.Collections.Generic;
using ClosedXML.Excel;

using Abp.UI;
using SimpleTaskApp.Net.MimeTypes;
using SimpleTaskApp.Roles.Dto;

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
                        ImageUrl = colorDto.ImageUrl,
                        StockQuantity = 0 

                    };
                    await _colorRepository.InsertAsync(color);
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return await GetAsync(new EntityDto<int>(phone.Id));
        }

    // ✅ UPDATE
    [AbpAuthorize(PermissionNames.Pages_MobilePhone_Edit)]

    public async Task<MobilePhoneDto> UpdateAsync(UpdateMobilePhoneDto input)
    {
      // Lấy mobile phone
      var phone = await _mobilePhoneRepository.GetAsync(input.Id);
      if (phone == null)
        throw new KeyNotFoundException("Mobile phone not found");

      // Cập nhật thông tin cơ bản
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

      // Chỉ cập nhật màu nếu có input.Colors
      if (input.Colors != null && input.Colors.Any())
      {
        var oldColors = await _colorRepository.GetAllListAsync(c => c.MobilePhoneId == phone.Id);

        // Xoá những màu không còn xuất hiện trong input
        foreach (var oldColor in oldColors)
        {
          if (!input.Colors.Any(c => c.ColorName == oldColor.ColorName && c.ColorHex == oldColor.ColorHex))
          {
            await _colorRepository.DeleteAsync(oldColor);
          }
        }

        // Thêm những màu mới
        foreach (var colorDto in input.Colors)
        {
          if (!oldColors.Any(c => c.ColorName == colorDto.ColorName && c.ColorHex == colorDto.ColorHex))
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
      }

      await CurrentUnitOfWork.SaveChangesAsync();

      // Trả về MobilePhoneDto mới nhất
      return await GetAsync(new EntityDto<int>(phone.Id));
    }

    // ✅ DELETE
    [AbpAuthorize]
    public async Task DeleteAsync(EntityDto<int> input)
    {
      var phone = await _mobilePhoneRepository.FirstOrDefaultAsync(input.Id);
      if (phone == null)
        return;
      // ================= Xóa điện thoại =================
      await _mobilePhoneRepository.DeleteAsync(phone);

      // ================= Commit =================
      await CurrentUnitOfWork.SaveChangesAsync();
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
    public async Task<PagedResultDto<MobilePhoneDto>> GetAllByUserAsync(PagedMobilePhoneResultRequestDto input)
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
        StockQuantity = c.StockQuantity, // ✅ Đảm bảo map đúng trường tồn kho
        MobilePhoneName = c.MobilePhone != null ? c.MobilePhone.Name : null
      }).ToList();

      return colorDtos;
    }



    public async Task<byte[]> ExportToExcelByFilterAsync(int? categoryId, DateTime? from, DateTime? to)
    {

      var query = _mobilePhoneRepository.GetAll()
          .Include(p => p.Colors)
          .Include(p => p.Category)
          .AsQueryable();

      query = query
            .WhereIf(categoryId.HasValue, p => p.CategoryId == categoryId.Value)
            .WhereIf(from.HasValue, p => p.CreationTime >= from.Value)
            .WhereIf(to.HasValue, p => p.CreationTime <= to.Value);

      var phones = await query.ToListAsync();

      var exportData = phones.Select(p => new
      {
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        DiscountPrice = p.DiscountPrice ?? 0,
        StockQuantity = p.StockQuantity,
        Brand = p.Brand,
        CategoryName = p.Category?.Name ?? "-",
        Status = GetStatus(p.IsNew, p.IsOnSale),
        SalePeriod = GetSalePeriod(p.SaleStart, p.SaleEnd),
        CreationTime = p.CreationTime,
        Colors = string.Join(", ", p.Colors.Select(c => $"{c.ColorName}({c.StockQuantity})"))
      }).ToList();

      using var workbook = new XLWorkbook();
      var ws = workbook.Worksheets.Add("MobilePhones");
      ws.Cell(1, 1).InsertTable(exportData);
      ws.Columns().AdjustToContents();

      using var ms = new MemoryStream();
      workbook.SaveAs(ms);
      return ms.ToArray();
    }


    // Hàm helper Status
    private string GetStatus(bool isNew, bool isOnSale)
  {
    if (isNew && isOnSale) return "New, OnSale";
    if (isNew) return "New";
    if (isOnSale) return "OnSale";
    return "-";
  }

  // Hàm helper SalePeriod
  private string GetSalePeriod(DateTime? start, DateTime? end)
  {
    if (start.HasValue && end.HasValue)
      return $"{start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
    return "-";
  }
    public async Task<int> ImportFromExcelAsync(Stream fileStream)
    {
      if (fileStream == null) throw new UserFriendlyException("File không được để trống.");

      int importedCount = 0;

      using (var workbook = new XLWorkbook(fileStream))
      {
        var ws = workbook.Worksheet(1); // Sheet đầu tiên
        var rows = ws.RowsUsed().Skip(1); // Bỏ qua header

        foreach (var row in rows)
        {
          // Đọc dữ liệu từ Excel và trim
          string name = row.Cell(1).GetValue<string>()?.Trim();
          string description = row.Cell(2).GetValue<string>()?.Trim();
          decimal price = row.Cell(3).GetValue<decimal>();
          decimal? discountPrice = row.Cell(4).GetValue<decimal?>();
          string brand = row.Cell(5).GetValue<string>()?.Trim();
          int categoryId = row.Cell(6).GetValue<int>();
          bool isNew = row.Cell(7).GetValue<bool>();
          bool isOnSale = row.Cell(8).GetValue<bool>();

          // Chuyển đổi string -> DateTime? để tránh lỗi InvalidCastException
          DateTime? saleStart = null;
          DateTime? saleEnd = null;
          string saleStartStr = row.Cell(9).GetValue<string>()?.Trim();
          string saleEndStr = row.Cell(10).GetValue<string>()?.Trim();

          if (!string.IsNullOrWhiteSpace(saleStartStr) && DateTime.TryParse(saleStartStr, out var start))
            saleStart = start;

          if (!string.IsNullOrWhiteSpace(saleEndStr) && DateTime.TryParse(saleEndStr, out var end))
            saleEnd = end;

          string colorsString = row.Cell(11).GetValue<string>()?.Trim();

          // Tạo entity MobilePhone
          var mobilePhone = new MobilePhone
          {
            Name = name,
            Description = description,
            Price = price,
            DiscountPrice = discountPrice,
            StockQuantity = 0,
            Brand = brand,
            CategoryId = categoryId,
            IsNew = isNew,
            IsOnSale = isOnSale,
            SaleStart = saleStart,
            SaleEnd = saleEnd,
            CreationTime = DateTime.Now
          };

          // Thêm MobilePhone vào DB để có Id
          await _mobilePhoneRepository.InsertAsync(mobilePhone);
          await CurrentUnitOfWork.SaveChangesAsync();

          // Xử lý màu sắc nếu có
          if (!string.IsNullOrWhiteSpace(colorsString))
          {
            var colors = colorsString.Split('|', StringSplitOptions.RemoveEmptyEntries);

            foreach (var color in colors)
            {
              var parts = color.Split(':', StringSplitOptions.RemoveEmptyEntries);
              if (parts.Length == 2)
              {
                var colorEntity = new MobilePhoneColor
                {
                  MobilePhoneId = mobilePhone.Id,
                  ColorName = parts[0].Trim(),
                  ColorHex = parts[1].Trim(),
                  StockQuantity = 0
                };
                await _colorRepository.InsertAsync(colorEntity);
              }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
          }

          importedCount++;
        }
      }

      return importedCount;
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
                    ImageUrl = c.ImageUrl,
                  StockQuantity = c.StockQuantity // <-- map tồn kho

                }).ToList()
            };

            return dto;
        }
    }
}
