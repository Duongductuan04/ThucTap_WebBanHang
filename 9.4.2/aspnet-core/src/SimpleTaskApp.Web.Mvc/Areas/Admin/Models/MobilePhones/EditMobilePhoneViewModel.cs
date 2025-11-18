using SimpleTaskApp.MobilePhones.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SimpleTaskApp.Areas.Admin.Models.MobilePhones
{
    public class EditMobilePhoneViewModel
    {
        // Dữ liệu MobilePhone cần edit
        public MobilePhoneDto MobilePhone { get; set; }

        // Dropdown Category
        public List<SelectListItem> Categories { get; set; }
    }
}
