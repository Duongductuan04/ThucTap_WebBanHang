using Abp.AutoMapper;
using SimpleTaskApp.MobilePhones;
using System;

namespace SimpleTaskApp.DiscountCategorys.Dto
{
    [AutoMapTo(typeof(DiscountCategory))]
    public class DiscountCategoryDto
    {
        public int Id { get; set; }              // Id của bản ghi
        public int CategoryId { get; set; }      // Id danh mục
        public string CategoryName { get; set; } // Tên danh mục
        public int DiscountId { get; set; }      // Id của Discount liên quan
    }
}
