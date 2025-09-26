using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppMobilePhones")]
    public class MobilePhone : Entity, IHasCreationTime
    {
        public string Name { get; set; }
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Giá gốc

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; } // Giá giảm (có thể null)

        public int StockQuantity { get; set; }

        public string Brand { get; set; } // Hãng sản xuất

        public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public MobilePhoneCategory Category { get; set; }

        public string ImageUrl { get; set; }

        public bool IsNew { get; set; } = false; // Sản phẩm mới
        public bool IsOnSale { get; set; } = false; // Khuyến mãi
        public DateTime? SaleStart { get; set; }
        public DateTime? SaleEnd { get; set; }

        public DateTime CreationTime { get; set; } = Clock.Now;
    }
}
