using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppMobilePhoneCategories")]
    public class MobilePhoneCategory : Entity
    {
        public string Name { get; set; } // Ví dụ: “Samsung”, “Apple”, “Laptop Dell”

        // Navigation
        public ICollection<MobilePhone> MobilePhones { get; set; }
    }
}
