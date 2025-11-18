using Abp.Application.Services;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.Statistics.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.Statistics
{
  public class StatisticsAppService : ApplicationService, IStatisticsAppService
  {
    private readonly IRepository<Order, int> _orderRepository;
    private readonly IRepository<OrderDetail, int> _orderDetailRepository;
    private readonly IRepository<MobilePhoneCategory, int> _categoryRepository;
    private readonly IRepository<MobilePhone, int> _mobilePhoneRepository;

    public StatisticsAppService(
        IRepository<Order, int> orderRepository,
        IRepository<OrderDetail, int> orderDetailRepository,
        IRepository<MobilePhoneCategory, int> categoryRepository,
            IRepository<MobilePhone, int> mobilePhoneRepository)  // Thêm dòng này

    {
      _orderRepository = orderRepository;
      _orderDetailRepository = orderDetailRepository;
      _categoryRepository = categoryRepository;
      _mobilePhoneRepository = mobilePhoneRepository;       // Gán

    }

    public async Task<StatisticsDto> GetDashboardStatisticsAsync(StatisticsFilterDto filter)
    {
      var now = DateTime.Now;
      int year = filter.Year ?? now.Year;
      int? month = filter.Month;
      int? day = filter.Day;

      // Tính khoảng thời gian tổng hợp cho filter (ngày/tháng/năm)
      DateTime startDate;
      DateTime endDate;
      if (day.HasValue && month.HasValue)
      {
        startDate = new DateTime(year, month.Value, day.Value);
        endDate = startDate.AddDays(1);
      }
      else if (month.HasValue)
      {
        startDate = new DateTime(year, month.Value, 1);
        endDate = startDate.AddMonths(1);
      }
      else
      {
        startDate = new DateTime(year, 1, 1);
        endDate = startDate.AddYears(1);
      }
      // Tổng sản phẩm đã bán
      var totalProductsSold = await _orderDetailRepository.GetAll()
          .SumAsync(od => (int?)od.Quantity) ?? 0;

      // Tổng đơn hàng
      var totalOrders = await _orderRepository.GetAll()
          .CountAsync();

      // Tổng khách hàng
      var totalCustomers = await _orderRepository.GetAll()
          .Select(o => o.UserId)
          .Distinct()
          .CountAsync();

      // Tổng doanh thu
      var totalRevenue = await _orderRepository.GetAll()
          .SumAsync(o => (decimal?)o.FinalAmount) ?? 0;


      // Lấy năm hiện tại
      int currentYear = DateTime.Now.Year;

      // Doanh thu theo 12 tháng
      var revenuesByMonth = new List<decimal>();
      for (int m = 1; m <= 12; m++)
      {
        var startOfMonth = new DateTime(currentYear, m, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var revenueMonth = await _orderRepository.GetAll()
            .Where(o => o.CreationTime >= startOfMonth && o.CreationTime < endOfMonth)
            .SumAsync(o => (decimal?)o.FinalAmount) ?? 0;

        revenuesByMonth.Add(revenueMonth);
      }


      // Doanh thu theo brand trong từng danh mục
      var revenueByCategory = await _orderDetailRepository.GetAll()
          .Include(od => od.Order)
          .Include(od => od.MobilePhone)
          .Where(od => od.Order.CreationTime >= startDate && od.Order.CreationTime < endDate)
          .ToListAsync();

      var categories = await _categoryRepository.GetAll().ToListAsync();

      var revenueByBrandPerCategory = revenueByCategory
          .GroupBy(od => od.MobilePhone.CategoryId)
          .Select(g =>
          {
            var cat = categories.FirstOrDefault(c => c.Id == g.Key);
            return new CategoryRevenueDto
            {
              CategoryId = g.Key,
              CategoryName = cat != null ? cat.Name : "Unknown",
              BrandRevenues = g.GroupBy(x => x.MobilePhone.Brand)
                               .Select(b => new BrandRevenueDto
                               {
                                 BrandName = b.Key,
                                 Revenue = b.Select(od => od.Order.FinalAmount).Sum()
                               }).ToList()
            };
          }).ToList();

      // ---------------------------
      // Top sản phẩm bán chạy nhất
      // ---------------------------
      var topProducts = await _orderDetailRepository.GetAll()
          .Include(od => od.MobilePhone)
          .Where(od => od.Order.CreationTime >= startDate && od.Order.CreationTime < endDate)
          .GroupBy(od => od.MobilePhoneId)
          .Select(g => new TopProductDto
          {
            MobilePhoneId = g.Key,
            ProductName = g.FirstOrDefault().MobilePhone.Name,
            QuantitySold = g.Sum(x => x.Quantity)
          })
          .OrderByDescending(tp => tp.QuantitySold)
          .Take(10) // Top 10 sản phẩm
          .ToListAsync();
      int lowStockThreshold = 10; // Sản phẩm nào <=5 cái sẽ báo động

      var lowStockProducts = await _mobilePhoneRepository.GetAll()
          .Where(p => p.StockQuantity <= lowStockThreshold)
          .OrderBy(p => p.StockQuantity)
          .Select(p => new LowStockProductDto
          {
            MobilePhoneId = p.Id,
            ProductName = p.Name,
            StockQuantity = p.StockQuantity
          })
          .ToListAsync();
      // ---------------------------
      // Top khách hàng tiềm năng
      // ---------------------------
      var topCustomers = await _orderRepository.GetAll()
          .Where(o => o.CreationTime >= startDate && o.CreationTime < endDate)
          .GroupBy(o => o.UserId)
        .Select(g => new TopCustomerDto
        {
          UserId = g.Key,
          UserName = g.FirstOrDefault().User.FullName,
          TotalOrders = g.Count(),
          TotalSpent = g.Sum(x => x.FinalAmount)
        })
          .OrderByDescending(tc => tc.TotalSpent)
          .Take(10) // Top 10 khách hàng
          .ToListAsync();
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
        TopCustomers = topCustomers // thêm vào đây


      };
    }
  }
}