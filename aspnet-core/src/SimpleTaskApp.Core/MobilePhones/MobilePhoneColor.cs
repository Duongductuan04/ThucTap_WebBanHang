using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppMobilePhoneColors")]
    public class MobilePhoneColor : Entity
    {
        public int MobilePhoneId { get; set; }

        [ForeignKey(nameof(MobilePhoneId))]
        public MobilePhone MobilePhone { get; set; }

        public string ColorName { get; set; } // Ví dụ: "Đen", "Trắng", "Xanh"
        public string ColorHex { get; set; }  // Mã màu dạng #000000 (tùy chọn)

        public string ImageUrl { get; set; } // ✅ Ảnh của màu đó
       public int StockQuantity { get; set; } // ✅ Số lượng tồn kho theo màu

  }
}
