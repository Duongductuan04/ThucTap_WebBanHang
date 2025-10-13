using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMap(typeof(MobilePhone))]
    public class MobilePhoneDto : IEntityDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public decimal Price { get; set; }             // Giá gốc
        public decimal? DiscountPrice { get; set; }     // Giá giảm (nếu có)

        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }       // Tên Category
        public string Brand { get; set; }              // Hãng sản xuất

        public string ImageUrl { get; set; }
        public IFormFile ImageFile { get; set; }       // Nếu upload từ form

        public bool IsNew { get; set; } = false;       // Sản phẩm mới
        public bool IsOnSale { get; set; } = false;   // Khuyến mãi
        public DateTime? SaleStart { get; set; }      // Bắt đầu khuyến mãi
        public DateTime? SaleEnd { get; set; }        // Kết thúc khuyến mãi
        public List<MobilePhoneColorDto> Colors { get; set; } = new List<MobilePhoneColorDto>();

        public DateTime CreationTime { get; set; }
    }
}
