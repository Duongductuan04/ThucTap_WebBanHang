using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using SimpleTaskApp.Authorization.Users;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppOrders")]
    public class Order : Entity, IHasCreationTime
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public DateTime OrderDate { get; set; } = Clock.Now;

        // ================== SỐ TIỀN ==================
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Tổng tiền gốc (chưa giảm, chưa phí ship)

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } // Giảm bao nhiêu tiền

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } // Phí vận chuyển

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; } // Tổng cuối cùng (Total - Discount + Shipping)

        // ================== THÔNG TIN NGƯỜI NHẬN ==================
        public string RecipientName { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientPhone { get; set; }

        // ================== PHƯƠNG THỨC THANH TOÁN ==================
        public int PaymentMethod { get; set; }
        // 0 = Tiền mặt, 1 = Chuyển khoản, 2 = Ví điện tử

        // ================== PHƯƠNG THỨC VẬN CHUYỂN ==================
        public int ShippingMethod { get; set; }
        // 0 = Giao hàng tiêu chuẩn, 1 = Giao nhanh, 2 = Giao siêu tốc

        // ================== TRẠNG THÁI ĐƠN HÀNG ==================
        /// <summary>
        /// 0: Pending, 1: Shipping, 2: Completed, 3: Cancelled
        /// </summary>
        public int Status { get; set; } = 0;

        // ================== VOUCHER ==================
        public int? DiscountId { get; set; } // Có thể null
        [ForeignKey(nameof(DiscountId))]
        public Discount Discount { get; set; }

        // ================== GHI CHÚ ==================
        public string Note { get; set; } // Người dùng ghi chú khi checkout

        // ================== NGÀY TẠO ==================
        public DateTime CreationTime { get; set; } = Clock.Now;

        // ================== CHI TIẾT ĐƠN HÀNG ==================
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
