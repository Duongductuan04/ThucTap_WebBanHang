using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.DiscountCategorys.Dto;
using SimpleTaskApp.DiscountProducts.Dto;
namespace SimpleTaskApp.MobilePhones.Dto
{
    // DTO tạo mới Discount
    [AutoMapTo(typeof(Discount))]
    public class CreateDiscountDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public decimal MinOrderValue { get; set; } = 0;
        public int ApplyType { get; set; }    // 0 = toàn bộ, 1 = theo danh mục, 2 = theo sản phẩm
        public int MaxUsage { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // ================== BẢNG TRUNG GIAN ==================
        public List<CreateDiscountCategoryDto> Categories { get; set; } = new List<CreateDiscountCategoryDto>();
        public List<CreateDiscountProductDto> Products { get; set; } = new List<CreateDiscountProductDto>();
    }

   

 

    // DTO cập nhật Discount, kế thừa từ CreateDiscountDto
    public class UpdateDiscountDto : CreateDiscountDto
    {
        public int Id { get; set; } // Id của Discount cần cập nhật
    }
}
