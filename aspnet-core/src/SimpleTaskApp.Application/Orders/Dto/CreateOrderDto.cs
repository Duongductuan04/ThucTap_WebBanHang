using Abp.AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMapTo(typeof(Order))]
    public class CreateOrderDto
    {
       // [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
     //   [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string RecipientName { get; set; }

        //[Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
     //   [StringLength(250, ErrorMessage = "Địa chỉ không được vượt quá 250 ký tự")]
        public string RecipientAddress { get; set; }

     //   [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
     //   [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (bắt đầu bằng 0, 10-11 số)")]
        public string RecipientPhone { get; set; }
        public int PaymentMethod { get; set; }
        public int ShippingMethod { get; set; }

        // Trạng thái mặc định Pending
        public int Status { get; set; } = 0;
        public decimal TotalAmount { get; set; }

        // Danh sách chi tiết đơn hàng
        public List<CreateOrderDetailDto> OrderDetails { get; set; } = new List<CreateOrderDetailDto>();
    }

    [AutoMapTo(typeof(OrderDetail))]
    public class CreateOrderDetailDto
    {
        public int MobilePhoneId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string MobilePhoneName { get; set; }
        public string ImageUrl { get; set; }
    }
}