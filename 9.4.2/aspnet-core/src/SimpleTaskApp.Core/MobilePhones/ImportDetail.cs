using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppImportDetails")]
    public class ImportDetail : Entity
    {
        public int ImportId { get; set; }

        [ForeignKey(nameof(ImportId))]
        public Import Import { get; set; }

        public int MobilePhoneId { get; set; }

        [ForeignKey(nameof(MobilePhoneId))]
        public MobilePhone MobilePhone { get; set; }

      public int? MobilePhoneColorId { get; set; } // ✅ Có thể null (nếu sp không có màu)
      [ForeignKey(nameof(MobilePhoneColorId))]
      public MobilePhoneColor MobilePhoneColor { get; set; }
    public int Quantity { get; set; } // Số lượng nhập
        [Column(TypeName = "decimal(18,2)")]
        public decimal ImportPrice { get; set; } // Giá nhập
    }
}
