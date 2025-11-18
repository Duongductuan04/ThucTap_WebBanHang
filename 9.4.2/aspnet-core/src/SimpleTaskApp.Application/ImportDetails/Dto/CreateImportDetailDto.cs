using Abp.AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMapTo(typeof(ImportDetail))]
    public class CreateImportDetailDto
    {
        [Required]
        public int MobilePhoneId { get; set; }
        public int? MobilePhoneColorId { get; set; } // ✅ Thêm vào


         [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal ImportPrice { get; set; }
    }
}
