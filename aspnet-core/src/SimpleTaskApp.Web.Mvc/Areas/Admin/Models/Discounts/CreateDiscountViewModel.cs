using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;

namespace SimpleTaskApp.Areas.Admin.Models.Discounts
{
    public class CreateDiscountViewModel
    {
        // Danh sách các category để chọn khi ApplyType = 1
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        // Danh sách sản phẩm để chọn khi ApplyType = 2
        public List<SelectListItem> Products { get; set; } = new List<SelectListItem>();

        // Thông tin Discount
        public CreateDiscountDto Discount { get; set; } = new CreateDiscountDto();

        // ApplyType: 0 = tất cả, 1 = danh mục, 2 = sản phẩm
        public int ApplyType
        {
            get => Discount.ApplyType;
            set => Discount.ApplyType = value;
        }

        // Constructor không tham số
        public CreateDiscountViewModel() { }

        // Constructor với danh sách category và sản phẩm
        public CreateDiscountViewModel(List<SelectListItem> categories, List<SelectListItem> products)
        {
            Categories = categories ?? new List<SelectListItem>();
            Products = products ?? new List<SelectListItem>();
        }
    }
}
