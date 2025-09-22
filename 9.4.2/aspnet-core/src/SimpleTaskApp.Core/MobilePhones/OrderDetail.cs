using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppOrderDetails")]
    public class OrderDetail : Entity
    {
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        public int MobilePhoneId { get; set; }
        [ForeignKey(nameof(MobilePhoneId))]
        public MobilePhone MobilePhone { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Lưu giá lúc đặt hàng

        // Bỏ ItemTotal vì có thể tính toán từ Quantity * UnitPrice
        // Chỉ cần thêm property tính toán trong ViewModel/DTO nếu cần hiển thị
    }
}