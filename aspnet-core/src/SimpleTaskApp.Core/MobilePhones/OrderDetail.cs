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
                                           // === Thêm màu sắc (nullable) ===
    public int? MobilePhoneColorId { get; set; } // Có thể null nếu sản phẩm không có màu
    [ForeignKey(nameof(MobilePhoneColorId))]
    public MobilePhoneColor MobilePhoneColor { get; set; }


  }
}
