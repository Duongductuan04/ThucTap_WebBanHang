using Abp.AutoMapper;
using SimpleTaskApp.MobilePhones;

namespace SimpleTaskApp.DiscountCategorys.Dto
{
    // DTO tạo mới DiscountCategory
    [AutoMapTo(typeof(DiscountCategory))]
    public class CreateDiscountCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // tên danh mục để hiển thị
    }

    // DTO cập nhật DiscountCategory
    [AutoMapTo(typeof(DiscountCategory))]
    public class UpdateDiscountCategoryDto : CreateDiscountCategoryDto
    {
        public int Id { get; set; } // Id của DiscountCategory cần update
    }
}
