using System.Collections.Generic;
using SimpleTaskApp.MobilePhones.Dto;
namespace SimpleTaskApp.Statistics.Dto
{
    // DTO cho doanh thu theo từng brand trong một danh mục
    public class BrandRevenueDto
    {
        public string BrandName { get; set; }   // Tên thương hiệu
        public decimal Revenue { get; set; }    // Doanh thu
    }

    // DTO cho danh mục + list brand revenue
    public class CategoryRevenueDto
    {
        public int CategoryId { get; set; }                       // Id danh mục
        public string CategoryName { get; set; }                  // Tên danh mục
        public List<BrandRevenueDto> BrandRevenues { get; set; } = new List<BrandRevenueDto>();
    }

    // DTO tổng hợp cho dashboard
    public class StatisticsDto
    {
        public int TotalProductsSold { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Doanh thu theo tháng (12 tháng)
        public List<decimal> RevenuesByMonth { get; set; } = new List<decimal>();
        //10 don hang moi nhat
        public List<LatestOrderDto> LatestOrders { get; set; } = new List<LatestOrderDto>();

        // Doanh thu theo brand trong từng danh mục
        public List<CategoryRevenueDto> RevenueByBrandPerCategory { get; set; } = new List<CategoryRevenueDto>();
    }
}
