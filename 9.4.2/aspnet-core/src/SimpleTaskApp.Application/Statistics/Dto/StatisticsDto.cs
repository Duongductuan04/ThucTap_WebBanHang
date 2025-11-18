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
  // DTO cho Top sản phẩm bán chạy
  public class TopProductDto
  {
    public int MobilePhoneId { get; set; }    // Id sản phẩm
    public string ProductName { get; set; }   // Tên sản phẩm
    public int QuantitySold { get; set; }     // Số lượng bán ra
  }
  // DTO cho Top sản phẩm tồn kho báo động
  public class LowStockProductDto
  {
    public int MobilePhoneId { get; set; }     // Id sản phẩm
    public string ProductName { get; set; }    // Tên sản phẩm
    public int StockQuantity { get; set; }     // Số lượng tồn
  }
  public class TopCustomerDto
  {
    public long UserId { get; set; }          // Id khách hàng
    public string UserName { get; set; }     // Tên khách hàng
    public int TotalOrders { get; set; }     // Tổng số đơn hàng
    public decimal TotalSpent { get; set; }  // Tổng chi tiêu
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

        // Doanh thu theo brand trong từng danh mục
        public List<CategoryRevenueDto> RevenueByBrandPerCategory { get; set; } = new List<CategoryRevenueDto>();
       public List<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();
    public List<LowStockProductDto> LowStockProducts { get; set; } = new List<LowStockProductDto>();
    public List<TopCustomerDto> TopCustomers { get; set; } = new List<TopCustomerDto>();



  }
}
