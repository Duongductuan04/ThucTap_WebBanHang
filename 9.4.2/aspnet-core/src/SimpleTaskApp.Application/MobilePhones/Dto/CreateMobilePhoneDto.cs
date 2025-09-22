using Abp.AutoMapper;
using Microsoft.AspNetCore.Http;
using System;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMapTo(typeof(MobilePhone))]
    public class CreateMobilePhoneDto
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public decimal Price { get; set; }             // Giá gốc
        public decimal? DiscountPrice { get; set; }    // Giá giảm (có thể null)

        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }            // Loại sản phẩm theo Category
        public string Brand { get; set; }              // Hãng sản xuất

        public string ImageUrl { get; set; }           // Nếu lưu url trực tiếp
        public IFormFile ImageFile { get; set; }       // Nếu upload từ form

        public bool IsNew { get; set; } = false;       // Sản phẩm mới
        public bool IsOnSale { get; set; } = false;    // Khuyến mãi
        public DateTime? SaleStart { get; set; }       // Bắt đầu khuyến mãi
        public DateTime? SaleEnd { get; set; }         // Kết thúc khuyến mãi
    }
}
