using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using SimpleTaskApp.Authorization.Users;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppCarts")]
    public class Cart : Entity
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public int MobilePhoneId { get; set; }
        [ForeignKey(nameof(MobilePhoneId))]
        public MobilePhone MobilePhone { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
