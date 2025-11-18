using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppDiscountProducts")]
    public class DiscountProduct : Entity
    {
        public int DiscountId { get; set; }
        [ForeignKey(nameof(DiscountId))]
        public Discount Discount { get; set; }

        public int MobilePhoneId { get; set; }
        [ForeignKey(nameof(MobilePhoneId))]
        public MobilePhone MobilePhone { get; set; }
    }
}
