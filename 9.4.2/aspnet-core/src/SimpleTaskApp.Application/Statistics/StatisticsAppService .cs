using Abp.Application.Services;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.Statistics.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
  
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

    public async Task<StatisticsDto> GetDashboardStatisticsAsync(StatisticsFilterDto filter)
    {
      // ================================
      // XỬ LÝ StartDate – EndDate
      // ================================
      DateTime startDate = DateTime.MinValue;
      DateTime endDate = DateTime.MaxValue;

      if (!string.IsNullOrWhiteSpace(filter.StartDate) &&
          !string.IsNullOrWhiteSpace(filter.EndDate))
      {
        startDate = DateTime.Parse(filter.StartDate);
        endDate = DateTime.Parse(filter.EndDate);
      }

      // ================================
      // TỔNG SẢN PHẨM ĐÃ BÁN
      // ================================
      var totalProductsSold = await _orderDetailRepository.GetAll()
     .SumAsync(x => (int?)x.Quantity) ?? 0;

      // ================================
      // TỔNG ĐƠN HÀNG
      // ================================
      var totalOrders = await _orderRepository.GetAll()
       .CountAsync();

      // ================================
      // TỔNG KHÁCH HÀNG
      // ================================
      var totalCustomers = await _orderRepository.GetAll()
        .Select(x => x.UserId)
        .Distinct()
        .CountAsync();

      // ================================
      // TỔNG DOANH THU
      // ================================
      var totalRevenue = await _orderRepository.GetAll()
       .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;

      // ================================
      // DOANH THU THEO 12 THÁNG
      // (không theo filter, vì đây là biểu đồ cả năm)
      // ================================
      int currentYear = DateTime.Now.Year;
      var revenuesByMonth = new List<decimal>();

      for (int m = 1; m <= 12; m++)
      {
        var mStart = new DateTime(currentYear, m, 1);
        var mEnd = mStart.AddMonths(1);

        var revenueMonth = await _orderRepository.GetAll()
            .Where(x => x.CreationTime >= mStart && x.CreationTime < mEnd)
            .SumAsync(x => (decimal?)x.FinalAmount) ?? 0;

        revenuesByMonth.Add(revenueMonth);
      }

      // ================================
      // DOANH THU THEO DANH MỤC → BRAND
      // ================================
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

      // ================================
      // TOP SẢN PHẨM BÁN CHẠY
      // ================================
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

      // ================================
      // SẢN PHẨM SẮP HẾT HÀNG
      // ================================
      int lowStockThreshold = 20;

      var lowStockProducts = await _mobilePhoneRepository.GetAll()
          .SelectMany(mp => mp.Colors.DefaultIfEmpty(), (mp, mc) => new LowStockProductVariantDto
          {
            MobilePhoneId = mp.Id,
            ProductName = mp.Name,
            ImageUrl = mp.ImageUrl,
            Color = mc != null ? mc.ColorName : null, // nếu không có màu => null
            ColorStockQuantity = mc != null ? mc.StockQuantity : 0, // nếu không có màu => 0
            TotalStockQuantity = mp.StockQuantity,
            LastImportDate = mc != null
                  ? _importDetailRepository.GetAll()
                      .Where(id => id.MobilePhoneId == mp.Id && id.MobilePhoneColorId == mc.Id)
                      .OrderByDescending(id => id.Import.CreationTime)
                      .Select(id => (DateTime?)id.Import.CreationTime)
                      .FirstOrDefault()
                  : null
          })
          .Where(x => (x.Color != null && x.ColorStockQuantity <= lowStockThreshold)
                   || (x.Color == null && x.TotalStockQuantity <= lowStockThreshold))
          .OrderBy(x => x.TotalStockQuantity)
          .ToListAsync();

      // ================================
      // TOP KHÁCH HÀNG
      // ================================
      // 1. Lấy tất cả orders (lọc theo ngày nếu có)
      var ordersQuery = _orderRepository.GetAll()
          .WhereIf(filter.StartDate != null && filter.EndDate != null,
                   o => o.CreationTime >= startDate && o.CreationTime <= endDate)
          .Select(o => new
          {
            o.UserId,
            o.RecipientName,
            o.RecipientPhone,
            o.RecipientAddress,
            o.FinalAmount,
            TotalProducts = o.OrderDetails.Sum(od => od.Quantity) // EF Core translate được
          });

      // 2. Load ra list trong memory
      var ordersList = await ordersQuery.ToListAsync();

      // 3. GroupBy theo khách hàng thực tế (tên + số điện thoại)
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

      // ================================
      // TRẢ VỀ DTO
      // ================================
      return new StatisticsDto
      {
        TotalProductsSold = totalProductsSold,
        TotalOrders = totalOrders,
        TotalCustomers = totalCustomers,
        MonthlyRevenue = totalRevenue,
        RevenuesByMonth = revenuesByMonth,
        RevenueByBrandPerCategory = revenueByBrandPerCategory,
        TopProducts = topProducts,
        LowStockProducts = lowStockProducts,
        TopCustomers = topCustomers
      };
    }
  }
}
