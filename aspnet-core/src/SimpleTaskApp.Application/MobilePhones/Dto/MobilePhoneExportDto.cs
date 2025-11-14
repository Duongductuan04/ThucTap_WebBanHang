using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones.Dto
{
  public class MobilePhoneExportDto
  {
    public string Name { get; set; }              // Tên sản phẩm
    public string Description { get; set; }       // Mô tả
    public decimal Price { get; set; }            // Giá gốc
    public decimal? DiscountPrice { get; set; }   // Giá giảm (nếu có)
    public int StockQuantity { get; set; }        // Tồn kho
    public string Brand { get; set; }             // Hãng sản xuất
    public string CategoryName { get; set; }      // Tên danh mục

    public string Status { get; set; }            // New, OnSale, hoặc New, OnSale
    public string SalePeriod { get; set; }        // Thời gian khuyến mãi
    public DateTime CreationTime { get; set; }    // Ngày tạo bản ghi

    public string Colors { get; set; }            // Tên màu + tồn kho. VD: Đen(10), Trắng(5)
  }
}
