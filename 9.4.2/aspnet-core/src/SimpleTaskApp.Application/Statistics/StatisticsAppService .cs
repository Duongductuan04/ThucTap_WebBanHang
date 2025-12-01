using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.Statistics.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Wordprocessing;
namespace SimpleTaskApp.Statistics
{
  public class StatisticsAppService : ApplicationService, IStatisticsAppService
  {
    private readonly IRepository<Order, int> _orderRepository;
    private readonly IRepository<OrderDetail, int> _orderDetailRepository;
    private readonly IRepository<MobilePhoneCategory, int> _categoryRepository;
    private readonly IRepository<MobilePhone, int> _mobilePhoneRepository;
    private readonly IRepository<MobilePhoneColor, int> _mobilePhoneColorRepository;
    private readonly IRepository<ImportDetail, int> _importDetailRepository;

    public StatisticsAppService(
        IRepository<Order, int> orderRepository,
        IRepository<OrderDetail, int> orderDetailRepository,
        IRepository<ImportDetail, int> importDetailRepository,
        IRepository<MobilePhoneColor, int> mobilePhoneColorRepository,
        IRepository<MobilePhoneCategory, int> categoryRepository,
        IRepository<MobilePhone, int> mobilePhoneRepository)
    {
      _orderRepository = orderRepository;
      _orderDetailRepository = orderDetailRepository;
      _importDetailRepository = importDetailRepository;
      _mobilePhoneColorRepository = mobilePhoneColorRepository;
      _categoryRepository = categoryRepository;
      _mobilePhoneRepository = mobilePhoneRepository;
    }

    // =========================
    // Hàm chính trả dashboard
    // =========================
    public async Task<StatisticsDto> GetDashboardStatisticsAsync(StatisticsFilterDto filter)
    {
      filter ??= new StatisticsFilterDto();

      // Xử lý StartDate-EndDate
      DateTime startDate = DateTime.MinValue;
      DateTime endDate = DateTime.MaxValue;

      if (!string.IsNullOrWhiteSpace(filter.StartDate) &&
          !string.IsNullOrWhiteSpace(filter.EndDate))
      {
        startDate = DateTime.Parse(filter.StartDate);
        endDate = DateTime.Parse(filter.EndDate).AddDays(1);
      }

      // Lọc order theo ngày
      var ordersInRange = _orderRepository.GetAll()
          .Where(o => o.CreationTime >= startDate && o.CreationTime < endDate);

      var totalRevenue = await ordersInRange.SumAsync(x => (decimal?)x.FinalAmount) ?? 0;
      var totalOrders = await ordersInRange.CountAsync();
      var totalCustomers = await ordersInRange.Select(x => x.UserId).Distinct().CountAsync();

      // Tổng sản phẩm bán ra
      var totalProductsSold = await _orderDetailRepository.GetAll()
          .Where(od => od.Order.CreationTime >= startDate && od.Order.CreationTime < endDate)
          .SumAsync(x => (int?)x.Quantity) ?? 0;

      // Biểu đồ doanh thu
      var (labels, data) = await GetRevenueChartAsync(startDate, endDate);

      // Doanh thu theo danh mục (brand)
      var revenueQuery = await _orderDetailRepository.GetAll()
          .Include(x => x.Order)
          .Include(x => x.MobilePhone)
          .WhereIf(filter.StartDate != null && filter.EndDate != null,
              x => x.Order.CreationTime >= startDate && x.Order.CreationTime <= endDate)
          .ToListAsync();

      var categories = await _categoryRepository.GetAll().ToListAsync();

      var revenueByBrandPerCategory = revenueQuery
          .GroupBy(x => x.MobilePhone.CategoryId)
          .Select(g =>
          {
            var cat = categories.FirstOrDefault(c => c.Id == g.Key);
            return new CategoryRevenueDto
            {
              CategoryId = g.Key,
              CategoryName = cat?.Name ?? "Unknown",
              BrandRevenues = g.GroupBy(x => x.MobilePhone.Brand)
                                       .Select(b => new BrandRevenueDto
                                   {
                                     BrandName = b.Key,
                                     Revenue = b.Sum(x => x.Quantity * x.UnitPrice)
                                   }).ToList()
            };
          }).ToList();

      // Top sản phẩm bán chạy
      var topProducts = await _orderDetailRepository.GetAll()
          .Include(x => x.MobilePhone)
          .Include(x => x.MobilePhoneColor)
          .WhereIf(filter.StartDate != null && filter.EndDate != null,
              x => x.Order.CreationTime >= startDate && x.Order.CreationTime <= endDate)
          .GroupBy(x => new
          {
            x.MobilePhoneId,
            x.MobilePhone.Name,
            x.MobilePhone.ImageUrl,
            x.MobilePhoneColorId,
            ColorName = x.MobilePhoneColor != null ? x.MobilePhoneColor.ColorName : null,
            ColorImageUrl = x.MobilePhoneColor != null ? x.MobilePhoneColor.ImageUrl : null
          })
          .Select(g => new TopProductDto
          {
            MobilePhoneId = g.Key.MobilePhoneId,
            ProductName = g.Key.Name,
            ImageUrl = g.Key.ImageUrl,
            MobilePhoneColorId = g.Key.MobilePhoneColorId,
            ColorName = g.Key.ColorName,
            ColorImageUrl = g.Key.ColorImageUrl,
            QuantitySold = g.Sum(x => x.Quantity)
          })
          .OrderByDescending(x => x.QuantitySold)
          .Take(20)
          .ToListAsync();

      // =========================
      // Tồn kho thấp
      // =========================
      var lowStockProducts = await GetLowStockProductsAsync(20);

      // Top khách hàng
      var ordersList = await _orderRepository.GetAll()
          .WhereIf(filter.StartDate != null && filter.EndDate != null,
              o => o.CreationTime >= startDate && o.CreationTime <= endDate)
          .Select(o => new
          {
            o.UserId,
            o.RecipientName,
            o.RecipientPhone,
            o.RecipientAddress,
            o.FinalAmount,
            TotalProducts = o.OrderDetails.Sum(od => od.Quantity)
          }).ToListAsync();

      var topCustomers = ordersList
          .GroupBy(o => new { o.RecipientPhone, o.RecipientName })
          .Select(g => new TopCustomerDto
          {
            UserName = g.Key.RecipientName,
            PhoneNumber = g.Key.RecipientPhone,
            Address = g.First().RecipientAddress,
            TotalOrders = g.Count(),
            TotalProducts = g.Sum(x => x.TotalProducts),
            TotalSpent = g.Sum(x => x.FinalAmount)
          })
          .OrderByDescending(x => x.TotalSpent)
          .Take(20)
          .ToList();

      // Trả về DTO
      return new StatisticsDto
      {
        TotalProductsSold = totalProductsSold,
        TotalOrders = totalOrders,
        TotalCustomers = totalCustomers,
        MonthlyRevenue = totalRevenue,
        RevenueChartLabels = labels,
        RevenueChartData = data,
        RevenueByBrandPerCategory = revenueByBrandPerCategory,
        TopProducts = topProducts,
        LowStockProducts = lowStockProducts,
        TopCustomers = topCustomers,
        Filter = filter
      };
    }

    // =========================
    // Hàm riêng: lấy sản phẩm tồn kho thấp
    // =========================
    public async Task<List<LowStockProductVariantDto>> GetLowStockProductsAsync(int lowStockThreshold = 20)
    {
      var mobiles = await _mobilePhoneRepository.GetAll()
          .Include(m => m.Colors)
          .ToListAsync();

      var importDetails = await _importDetailRepository.GetAll()
          .Include(id => id.Import)
          .ToListAsync();

      var lowStockProducts = mobiles
          .SelectMany(mp => mp.Colors.DefaultIfEmpty(), (mp, mc) =>
          {
            DateTime? lastImportDate = null;
            if (mc != null)
            {
              lastImportDate = importDetails
                        .Where(id => id.MobilePhoneId == mp.Id && id.MobilePhoneColorId == mc.Id)
                        .OrderByDescending(id => id.Import.CreationTime)
                        .FirstOrDefault()?.Import.CreationTime;
            }

            return new LowStockProductVariantDto
            {
              MobilePhoneId = mp.Id,
              ProductName = mp.Name,
              ImageUrl = mp.ImageUrl,
              Color = mc?.ColorName,
              ColorStockQuantity = mc?.StockQuantity ?? 0,
              TotalStockQuantity = mp.StockQuantity,
              LastImportDate = lastImportDate
            };
          })
          .Where(x => (x.Color != null && x.ColorStockQuantity <= lowStockThreshold)
                      || (x.Color == null && x.TotalStockQuantity <= lowStockThreshold))
          .OrderBy(x => x.TotalStockQuantity)
          .ToList();

      return lowStockProducts;
    }

    // =========================
    // Biểu đồ doanh thu
    // =========================
    private async Task<(List<string> labels, List<decimal> data)> GetRevenueChartAsync(DateTime startDate, DateTime endDate)
    {
      var totalDays = (endDate - startDate).TotalDays + 1;
      var labels = new List<string>();
      var data = new List<decimal>();

      if (totalDays <= 8)
      {
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
          labels.Add(d.ToString("dd/MM"));
          var revenue = await _orderRepository.GetAll()
              .Where(x => x.CreationTime.Date == d.Date)
              .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;
          data.Add(revenue);
        }
      }
      else if (totalDays <= 30)
      {
        var d = startDate;
        while (d <= endDate)
        {
          var dEnd = d.AddDays(1) <= endDate ? d.AddDays(1) : endDate;
          labels.Add($"{d:dd/MM}-{dEnd:dd/MM}");
          var revenue = await _orderRepository.GetAll()
              .Where(x => x.CreationTime.Date >= d.Date && x.CreationTime.Date <= dEnd.Date)
              .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;
          data.Add(revenue);
          d = dEnd.AddDays(1);
        }
      }
      else if (totalDays <= 90)
      {
        var d = startDate;
        while (d <= endDate)
        {
          var dEnd = d.AddDays(7) <= endDate ? d.AddDays(7) : endDate;
          labels.Add($"{d:dd/MM}-{dEnd:dd/MM}");
          var revenue = await _orderRepository.GetAll()
              .Where(x => x.CreationTime.Date >= d.Date && x.CreationTime.Date <= dEnd.Date)
              .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;
          data.Add(revenue);
          d = dEnd.AddDays(1);
        }
      }
      else
      {
        var d = new DateTime(startDate.Year, startDate.Month, 1);
        var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
        while (d <= endMonth)
        {
          var monthEnd = d.AddMonths(1).AddDays(-1);
          labels.Add(d.ToString("MM/yyyy"));
          var revenue = await _orderRepository.GetAll()
              .Where(x => x.CreationTime.Date >= d.Date && x.CreationTime.Date <= monthEnd.Date)
              .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;
          data.Add(revenue);
          d = d.AddMonths(1);
        }
      }

      return (labels, data);
    }

    public async Task<byte[]> ExportTopProductsToExcelAsync(ExportTopProductsInput input)
    {
      var topProducts = input.TopProducts;
      var filter = input.Filter;

      using var workbook = new XLWorkbook();
      var ws = workbook.Worksheets.Add("Top Products");

      ws.Cell(1, 1).Value = "Báo cáo Top sản phẩm bán chạy";
      ws.Cell(2, 1).Value = $"Từ: {filter.StartDate ?? "--"}  Đến: {filter.EndDate ?? "--"}";
      ws.Range("A1:D1").Merge().Style.Font.Bold = true;//tô đậm
      ws.Range("A2:D2").Merge().Style.Font.Italic = true;//in nghiêng

      ws.Cell(4, 1).Value = "STT";
      ws.Cell(4, 2).Value = "Tên sản phẩm";
      ws.Cell(4, 3).Value = "Màu";
      ws.Cell(4, 4).Value = "Số lượng bán";
      //tiêu đề in đậm và nền xám
      ws.Range("A4:D4").Style.Font.Bold = true;
      ws.Range("A4:D4").Style.Fill.BackgroundColor = XLColor.LightGray;
      // Dữ liệu sản phẩm
      for (int i = 0; i < topProducts.Count; i++)
      {
        var p = topProducts[i];
        ws.Cell(5 + i, 1).Value = i + 1;
        ws.Cell(5 + i, 2).Value = p.ProductName;
        ws.Cell(5 + i, 3).Value = p.ColorName ?? "";
        ws.Cell(5 + i, 4).Value = p.QuantitySold;
        ws.Cell(5 + i, 4).Style.NumberFormat.Format = "#,##0";
      }
      // Auto-fit cột
      ws.Columns().AdjustToContents();

      using var stream = new MemoryStream();
      workbook.SaveAs(stream);
      return stream.ToArray();
    }
    public async Task<byte[]> ExportTopCustomersToExcelAsync(ExportTopCustomersInput input)
    {
      var topCustomers = input.TopCustomers;
      var filter = input.Filter;
      using var workbook = new XLWorkbook();
       var ws = workbook.Worksheets.Add("Top Customers");
      ws.Cell(1, 1).Value = "Báo cáo Top khách hàng";
      ws.Cell(2, 1).Value = $"Từ: {filter.StartDate ?? "--"}  Đến: {filter.EndDate ?? "--"}";
      ws.Range("A1:E1").Merge().Style.Font.Bold = true;//tô đậm
      ws.Range("A2:E2").Merge().Style.Font.Italic = true;//in nghiêng
      ws.Cell(4, 1).Value = "STT";
      ws.Cell(4, 2).Value = "Tên khách hàng";
      ws.Cell(4, 3).Value = "Số điện thoại";
      ws.Cell(4, 4).Value = "Địa chỉ";
      ws.Cell(4, 5).Value = "Tổng đơn hàng";
      ws.Cell(4, 6).Value = "Tổng sản phẩm mua";
      ws.Cell(4, 7).Value = "Tổng chi tiêu";
      //tiêu đề in đậm và nền xám
      ws.Range("A4:G4").Style.Font.Bold = true;
      ws.Range("A4:G4").Style.Fill.BackgroundColor = XLColor.LightGray;
      // Dữ liệu khách hàng
      for (int i = 0; i < topCustomers.Count; i++)
      {
        var c = topCustomers[i];
        int row = 5 + i;
        ws.Cell(row, 1).Value = i + 1;
        ws.Cell(row, 2).Value = c.UserName;
        ws.Cell(row, 3).Value = c.PhoneNumber;
        ws.Cell(row, 4).Value = c.Address;
        ws.Cell(row, 5).Value = c.TotalOrders;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
        ws.Cell(row, 6).Value = c.TotalProducts;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
        ws.Cell(row, 7).Value = c.TotalSpent;
        ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0 \"₫\"";
      }
      // Auto-fit cột
      ws.Columns().AdjustToContents();
      //Export
      using var stream = new MemoryStream();
      workbook.SaveAs(stream);
      return stream.ToArray();
    }

    public async Task<byte[]> ExportLowStockProductsToExcelAsync(List<LowStockProductVariantDto> items)
    { using var workbook = new XLWorkbook();
      var ws = workbook.Worksheets.Add("Low Stock Products");
      ws.Cell(1, 1).Value = "Báo cáo sản phẩm tồn kho thấp";
      ws.Range("A1:E1").Merge().Style.Font.Bold = true;//tô đậm
      ws.Cell(3, 1).Value = "STT";
      ws.Cell(3, 2).Value = "Tên sản phẩm";
      ws.Cell(3, 3).Value = "Màu";
      ws.Cell(3, 4).Value = "Tồn kho màu";
      ws.Cell(3, 5).Value = "Tổng tồn kho";
      ws.Cell(3, 6).Value = "Ngày nhập cuối";
      //tiêu đề in đậm và nền xám
      ws.Range("A3:F3").Style.Font.Bold = true;
      ws.Range("A3:F3").Style.Fill.BackgroundColor = XLColor.LightGray;
      // Dữ liệu sản phẩm
      for (int i = 0; i < items.Count; i++)
      {
        var p = items[i];
        int row = 4 + i;
        ws.Cell(row, 1).Value = i + 1;
        ws.Cell(row, 2).Value = p.ProductName;
        ws.Cell(row, 3).Value = p.Color ?? "";
        ws.Cell(row, 4).Value = p.ColorStockQuantity;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
        ws.Cell(row, 5).Value = p.TotalStockQuantity;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
        ws.Cell(row, 6).Value = p.LastImportDate?.ToString("dd/MM/yyyy") ?? "N/A";
      }
      // Auto-fit cột
      ws.Columns().AdjustToContents();
      //Export
      using var stream = new MemoryStream();
      workbook.SaveAs(stream);
      return stream.ToArray();
    }
    public async Task<byte[]> ExportCategoryRevenueToExcelAsync(ExportCategoryRevenueInput input)
    {
      var categories = input.Categories;
      var filter = input.Filter;

      using var workbook = new XLWorkbook();
      var ws = workbook.Worksheets.Add("Doanh Thu Theo Danh Mục");

      // ===== TIÊU ĐỀ =====
      ws.Cell(1, 1).Value = "Báo cáo doanh thu theo danh mục & thương hiệu";
      ws.Cell(2, 1).Value = $"Từ: {filter.StartDate ?? "--"}  Đến: {filter.EndDate ?? "--"}";
      ws.Range("A1:D1").Merge().Style.Font.Bold = true;
      ws.Range("A2:D2").Merge().Style.Font.Italic = true;

      // ===== HEADER =====
      ws.Cell(4, 1).Value = "STT";
      ws.Cell(4, 2).Value = "Danh mục";
      ws.Cell(4, 3).Value = "Thương hiệu";
      ws.Cell(4, 4).Value = "Doanh thu";

      var headerRange = ws.Range("A4:D4");
      headerRange.Style.Font.Bold = true;
      headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
      headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

      // ===== DỮ LIỆU =====
      int row = 5;
      int stt = 1;

      foreach (var category in categories)
      {
        int categoryStartRow = row; // dòng bắt đầu merge cho danh mục
        foreach (var brand in category.BrandRevenues)
        {
          ws.Cell(row, 1).Value = stt++;
          ws.Cell(row, 3).Value = brand.BrandName;
          ws.Cell(row, 4).Value = brand.Revenue;
          ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0 \"₫\"";
          row++;
        }

        // Merge ô danh mục nếu có nhiều thương hiệu
        if (category.BrandRevenues.Count > 1)
        {
          ws.Range(categoryStartRow, 2, row - 1, 2).Merge();
          ws.Cell(categoryStartRow, 2).Value = category.CategoryName;
          ws.Cell(categoryStartRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
          ws.Cell(categoryStartRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
        else if (category.BrandRevenues.Count == 1)
        {
          ws.Cell(categoryStartRow, 2).Value = category.CategoryName;
          ws.Cell(categoryStartRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
      }

      // ===== AUTO-FIT CỘT =====
      ws.Columns().AdjustToContents();

      // ===== RETURN BYTE[] =====
      using var stream = new MemoryStream();
      workbook.SaveAs(stream);
      return stream.ToArray();
    }


    public async Task<byte[]> ExportStatisticsToExcelAsync(StatisticsDto stats)
    {
      using var workbook = new XLWorkbook();
      var ws = workbook.Worksheets.Add("Dashboard Statistics");

      // Header thông tin chung
      ws.Cell(1, 1).Value = "Thông tin chung";
      ws.Cell(2, 1).Value = "Tổng sản phẩm bán ra";
      ws.Cell(2, 2).Value = stats.TotalProductsSold;

      ws.Cell(3, 1).Value = "Tổng đơn hàng";
      ws.Cell(3, 2).Value = stats.TotalOrders;

      ws.Cell(4, 1).Value = "Tổng khách hàng";
      ws.Cell(4, 2).Value = stats.TotalCustomers;

      ws.Cell(5, 1).Value = "Doanh thu tháng";
      ws.Cell(5, 2).Value = stats.MonthlyRevenue;
      ws.Cell(5, 2).Style.NumberFormat.Format = "#,##0 \"₫\"";

      // Biểu đồ doanh thu (dạng bảng)
      ws.Cell(7, 1).Value = "Biểu đồ doanh thu";
      ws.Cell(8, 1).Value = "Ngày/Tháng";
      ws.Cell(8, 2).Value = "Doanh thu";

      for (int i = 0; i < stats.RevenueChartLabels.Count; i++)
      {
        ws.Cell(9 + i, 1).Value = stats.RevenueChartLabels[i];
        ws.Cell(9 + i, 2).Value = stats.RevenueChartData[i];
        ws.Cell(9 + i, 2).Style.NumberFormat.Format = "#,##0 \"₫\"";
      }

      // Format header
      ws.Range("A1:B1").Style.Font.Bold = true;
      ws.Range("A8:B8").Style.Font.Bold = true;
      ws.Range("A8:B8").Style.Fill.BackgroundColor = XLColor.LightGray;

      // Auto-fit cột
      ws.Columns().AdjustToContents();

      using var stream = new MemoryStream();
      workbook.SaveAs(stream);
      return await Task.FromResult(stream.ToArray());
    }
  }
}
