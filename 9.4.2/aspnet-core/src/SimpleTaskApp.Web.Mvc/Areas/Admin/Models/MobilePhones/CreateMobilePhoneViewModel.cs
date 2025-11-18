using SimpleTaskApp.MobilePhones.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SimpleTaskApp.Areas.Admin.Models.MobilePhones
{
    public class CreateMobilePhoneViewModel
    {
        // Dropdown Category
        public List<SelectListItem> Categories { get; set; }

        // Constructor
        public CreateMobilePhoneViewModel(List<SelectListItem> categories)
        {
            Categories = categories;
        }

        // Parameterless constructor (nếu cần tạo model rỗng)
        public CreateMobilePhoneViewModel()
        {
            Categories = new List<SelectListItem>();
        }
    }
}
