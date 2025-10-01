using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMapFrom(typeof(Order))]
    public class OrderDto : EntityDto<int>
    {
        public long UserId { get; set; }
        public string UserName { get; set; } // nếu muốn hiển thị tên user

        // ================== THÔNG TIN NGƯỜI NHẬN ==================
        public string RecipientName { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientPhone { get; set; }

        // ================== PHƯƠNG THỨC ==================
        public int PaymentMethod { get; set; }    // 0 = Tiền mặt, 1 = Chuyển khoản, 2 = Ví điện tử
        public int ShippingMethod { get; set; }   // 0 = Tiêu chuẩn, 1 = Nhanh, 2 = Siêu tốc

        // ================== TRẠNG THÁI ==================
        public int Status { get; set; }           // 0 = Pending, 1 = Shipping, 2 = Completed, 3 = Cancelled

        // ================== TIỀN TỆ ==================
        public decimal TotalAmount { get; set; }      // Tổng tiền gốc
        public decimal? DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }      // Phí ship
        public decimal FinalAmount { get; set; }      // Tổng cuối cùng

        // ================== VOUCHER ==================
        public int? DiscountId { get; set; }
        public string DiscountCode { get; set; }          // Mã voucher
        public decimal? DiscountPercentage { get; set; }  // % giảm trực tiếp từ Discount

        // ================== GHI CHÚ ==================
        public string Note { get; set; }

        // ================== NGÀY ==================
        public DateTime OrderDate { get; set; }   // Ngày đặt
        public DateTime CreationTime { get; set; }

        // ================== CHI TIẾT ĐƠN HÀNG ==================
        public List<OrderDetailDto> OrderDetails { get; set; }

        // ================== TIỆN ÍCH ==================
        // Có thể thêm thuộc tính tính toán (không map DB)
        public decimal CalculatedTotal => OrderDetails?.Sum(od => od.ItemTotal) ?? 0;
    }
}
