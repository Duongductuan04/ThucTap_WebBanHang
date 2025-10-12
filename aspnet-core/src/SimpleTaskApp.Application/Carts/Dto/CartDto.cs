using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMap(typeof(Cart))]  
    public class CartDto : EntityDto<int>
    {
        public long UserId { get; set; }
        public int MobilePhoneId { get; set; }

        public string Name { get; set; }        // Tên sản phẩm
        public string ImageUrl { get; set; }    // Ảnh sản phẩm
        public decimal Price { get; set; }      // Giá gốc
        public decimal? DiscountPrice { get; set; } // Giá khuyến mãi (nếu có)
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsOutOfStock { get; set; }
        public string StatusMessage { get; set; }
        // Giá hiển thị: ưu tiên giá khuyến mãi
        public decimal DisplayPrice { get; set; }

        // Thành tiền = giá hiển thị * số lượng
        public int? StockQuantity { get; set; }
        public string Message { get; set; }

        public decimal Total => Quantity * DisplayPrice;
    }
}
