using Abp.AutoMapper;
using SimpleTaskApp.MobilePhones;

namespace SimpleTaskApp.DiscountProducts.Dto
{
    // DTO tạo mới DiscountProduct
    [AutoMapTo(typeof(DiscountProduct))]
    public class CreateDiscountProductDto
    {
        public int MobilePhoneId { get; set; }
        public string MobilePhoneName { get; set; } // tên sản phẩm để hiển thị
    }

    // DTO cập nhật DiscountProduct
    [AutoMapTo(typeof(DiscountProduct))]
    public class UpdateDiscountProductDto : CreateDiscountProductDto
    {
        public int Id { get; set; } // Id của DiscountProduct cần update
    }
}
