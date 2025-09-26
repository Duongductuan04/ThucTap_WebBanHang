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

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Lưu tổng tiền vào DB

        public string RecipientName { get; set; }
        public string RecipientAddress { get; set; }

        // Thêm số điện thoại người nhận
        public string RecipientPhone { get; set; }

        // Phương thức thanh toán
        public int PaymentMethod { get; set; }
        // 0 = Tiền mặt
        // 1 = Chuyển khoản
        // 2 = Ví điện tử

        // Thêm phương thức vận chuyển
        public int ShippingMethod { get; set; }
        // 0 = Giao hàng tiêu chuẩn
        // 1 = Giao hàng nhanh
        // 2 = Giao hàng siêu tốc

        /// <summary>
        /// 0: Pending, 1: Shipping, 2: Completed, 3: Cancelled
        /// </summary>
        public int Status { get; set; } = 0;

        public DateTime CreationTime { get; set; } = Clock.Now;

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}