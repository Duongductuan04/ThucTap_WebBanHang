using System.Collections.Generic;
using DocumentFormat.OpenXml.Wordprocessing;
using SimpleTaskApp.MobilePhones.Dto;
using System;
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
  public class TopProductDto
  {
    public int MobilePhoneId { get; set; }        // Id sản phẩm
    public string ProductName { get; set; }        // Tên sản phẩm
    public string ImageUrl { get; set; }           // Ảnh sản phẩm (ảnh chính)

    public int? MobilePhoneColorId { get; set; }   // Id màu người mua đã chọn
    public string ColorName { get; set; }          // Tên màu người mua đã chọn
    public string ColorImageUrl { get; set; }      // Ảnh màu (nếu có)

    public int QuantitySold { get; set; }          // Số lượng đã bán theo từng màu
  }
  // DTO cho Top sản phẩm tồn kho báo động
  public class LowStockProductVariantDto
  {
    public int MobilePhoneId { get; set; }
    public string ProductName { get; set; }
    public string ImageUrl { get; set; }
    public string Color { get; set; }  // màu
    public int ColorStockQuantity { get; set; } // tồn kho màu
    public int TotalStockQuantity { get; set; } // tổng tồn kho sản phẩm
    public DateTime? LastImportDate { get; set; } // ngày nhập cuối cùng
  }

  // DTO mới
  public class TopCustomerDto
  {
    public long UserId { get; set; }          // Id khách hàng
    public string UserName { get; set; }      // Tên khách hàng
    public string PhoneNumber { get; set; }   // Số điện thoại
    public string Address { get; set; }       // Địa chỉ
    public int TotalOrders { get; set; }      // Tổng số đơn hàng
    public int TotalProducts { get; set; }    // Tổng số sản phẩm đã mua
    public decimal TotalSpent { get; set; }   // Tổng chi tiêu
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
    public List<LowStockProductVariantDto> LowStockProducts { get; set; } = new List<LowStockProductVariantDto>();
    public List<TopCustomerDto> TopCustomers { get; set; } = new List<TopCustomerDto>();



  }
}
