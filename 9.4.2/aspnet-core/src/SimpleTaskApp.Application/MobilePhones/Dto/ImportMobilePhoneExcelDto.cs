using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimpleTaskApp.MobilePhones.Dto
{
  public class ImportMobilePhoneExcelDto
  {
    [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
    [MaxLength(256, ErrorMessage = "Tên tối đa 256 ký tự.")]
    [RegularExpression("^[a-zA-Z0-9\\s]+$", ErrorMessage = "Tên chỉ cho phép chữ và số.")]
    public string Name { get; set; }

    [MaxLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Giá không được để trống.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    public int StockQuantity { get; set; } = 0;

    [Required(ErrorMessage = "Thương hiệu không được để trống.")]
    [MaxLength(100, ErrorMessage = "Thương hiệu tối đa 100 ký tự.")]
    public string Brand { get; set; }

    [Required(ErrorMessage = "Danh mục không được để trống.")]
    public int CategoryId { get; set; }

    public bool IsNew { get; set; }
    public bool IsOnSale { get; set; }

    public DateTime? SaleStart { get; set; }
    public DateTime? SaleEnd { get; set; }

    public string Colors { get; set; }  // "Đen:#000000|Trắng:#FFFFFF"
  }
}
