
using Abp.AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimpleTaskApp.MobilePhones.Dto
{
  [AutoMapTo(typeof(Order))]
  public class CreateOrderDto
  {
    // ================== THÔNG TIN NGƯỜI NHẬN ==================
    [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
    [StringLength(100, ErrorMessage = "Tên người nhận tối đa 100 ký tự")]
    [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Tên người nhận chỉ được chứa chữ cái và khoảng trắng")]
    public string RecipientName { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
    [RegularExpression(@"^[a-zA-Z0-9À-ỹ\s,.\-]+$", ErrorMessage = "Địa chỉ không hợp lệ")]
    public string RecipientAddress { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số")]
    public string RecipientPhone { get; set; }

    // ================== PHƯƠNG THỨC ==================
    public int PaymentMethod { get; set; }   // 0 = Tiền mặt, 1 = Chuyển khoản, 2 = Ví điện tử

    public int ShippingMethod { get; set; }  // 0 = Tiêu chuẩn, 1 = Nhanh, 2 = Siêu tốc

    // ================== TRẠNG THÁI ==================
    public int Status { get; set; } = 0;     // Mặc định Pending

    // ================== VOUCHER ==================
    public int? DiscountId { get; set; }

    [StringLength(50, ErrorMessage = "Mã giảm giá tối đa 50 ký tự")]
    public string DiscountCode { get; set; }

    // ================== GHI CHÚ ==================
    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string Note { get; set; }

    // ================== TỔNG TIỀN ==================
    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal FinalAmount { get; set; }

    // ================== DANH SÁCH SẢN PHẨM ==================
    public List<CreateOrderDetailDto> OrderDetails { get; set; } = new List<CreateOrderDetailDto>();
  }

  [AutoMapTo(typeof(OrderDetail))]
  public class CreateOrderDetailDto
  {
    public int MobilePhoneId { get; set; }
    public int Quantity { get; set; }
    public int? MobilePhoneColorId { get; set; } // ⚙ đổi tên cho khớp entity
    public decimal UnitPrice { get; set; }   // Giá tại thời điểm đặt
    public string MobilePhoneName { get; set; }
    public string ImageUrl { get; set; }
    public string ColorName { get; set; }
    public string ColorImageUrl { get; set; }
    public int ColorStockQuantity { get; set; } 

  }
}
