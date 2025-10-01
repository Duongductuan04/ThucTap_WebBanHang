using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;

namespace SimpleTaskApp.Areas.Admin.Models.Discounts
{
    public class EditDiscountViewModel
    {
        // Danh sách các category để chọn khi ApplyType = 1
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        // Danh sách sản phẩm để chọn khi ApplyType = 2
        public List<SelectListItem> Products { get; set; } = new List<SelectListItem>();

        // Thông tin Discount
        public DiscountDto Discount { get; set; } = new DiscountDto();

        // ApplyType: 0 = tất cả, 1 = danh mục, 2 = sản phẩm
        public int ApplyType
        {
            get => Discount.ApplyType;
            set => Discount.ApplyType = value;
        }

        // IDs các category đã chọn
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

        // IDs các product đã chọn
        public List<int> SelectedProductIds { get; set; } = new List<int>();

        // Constructor không tham số
        public EditDiscountViewModel() { }

        // Constructor với danh sách category, sản phẩm và discount
        public EditDiscountViewModel(List<SelectListItem> categories, List<SelectListItem> products, DiscountDto discount)
        {
            Categories = categories ?? new List<SelectListItem>();
            Products = products ?? new List<SelectListItem>();
            Discount = discount ?? new DiscountDto();
        }
    }
}
