using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppDiscountCategories")]
    public class DiscountCategory : Entity
    {
        public int DiscountId { get; set; }
        [ForeignKey(nameof(DiscountId))]
        public Discount Discount { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public MobilePhoneCategory Category { get; set; }
    }
}
