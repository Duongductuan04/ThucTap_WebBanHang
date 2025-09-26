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

        public StatisticsAppService(
            IRepository<Order, int> orderRepository,
            IRepository<OrderDetail, int> orderDetailRepository,
            IRepository<MobilePhoneCategory, int> categoryRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _categoryRepository = categoryRepository;
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

            // Tổng sản phẩm, đơn hàng, khách hàng, doanh thu
            var totalProductsSold = await _orderDetailRepository.GetAll()
                .Where(od => od.Order.CreationTime >= startDate && od.Order.CreationTime < endDate)
                .SumAsync(od => (int?)od.Quantity) ?? 0;

            var totalOrders = await _orderRepository.GetAll()
                .Where(o => o.CreationTime >= startDate && o.CreationTime < endDate)
                .CountAsync();

            var totalCustomers = await _orderRepository.GetAll()
                .Where(o => o.CreationTime >= startDate && o.CreationTime < endDate)
                .Select(o => o.UserId)
                .Distinct()
                .CountAsync();

            var totalRevenue = await _orderRepository.GetAll()
                .Where(o => o.CreationTime >= startDate && o.CreationTime < endDate)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Doanh thu theo 12 tháng (cột)
            var revenuesByMonth = new List<decimal>();
            for (int m = 1; m <= 12; m++)
            {
                var startOfMonth = new DateTime(year, m, 1);
                var endOfMonth = startOfMonth.AddMonths(1);

                var revenueMonth = await _orderRepository.GetAll()
                    .Where(o => o.CreationTime >= startOfMonth && o.CreationTime < endOfMonth)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

                revenuesByMonth.Add(revenueMonth);
            }

            // ---------------------------
            // Doanh thu theo brand trong từng danh mục (lọc theo năm)
            // ---------------------------
            var startOfYear = new DateTime(year, 1, 1);
            var endOfYear = startOfYear.AddYears(1);

            var revenueByCategory = await _orderDetailRepository.GetAll()
                .Include(od => od.Order)
                .Include(od => od.MobilePhone)
                .Where(od => od.Order.CreationTime >= startOfYear && od.Order.CreationTime < endOfYear)
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
                                             Revenue = b.Sum(x => x.Quantity * x.MobilePhone.Price)
                                         }).ToList()
                    };
                }).ToList();
            // Lấy 10 đơn hàng mới nhất cùng chi tiết sản phẩm
            var latestOrdersDto = await _orderRepository.GetAll()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MobilePhone)
                .OrderByDescending(o => o.CreationTime)
                .Take(10)
                .Select(o => new LatestOrderDto
                {
                    OrderId = o.Id,
                    Status = o.Status,
                    CreationTime = o.CreationTime,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderDetails.Select(od => new OrderItemDto
                    {
                        MobilePhoneName = od.MobilePhone.Name,
                        Quantity = od.Quantity,
                        UnitPrice = od.MobilePhone.Price
                    }).ToList()
                })
                .ToListAsync();
            return new StatisticsDto
            {
                TotalProductsSold = totalProductsSold,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                MonthlyRevenue = totalRevenue,
                RevenuesByMonth = revenuesByMonth,
                RevenueByBrandPerCategory = revenueByBrandPerCategory,
                LatestOrders = latestOrdersDto

            };
        }
    }
}
