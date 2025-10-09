using SimpleTaskApp.MobilePhones.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SimpleTaskApp.Areas.Admin.Models.Imports
{
    // ViewModel tạo phiếu nhập
    public class CreateImportViewModel
    {
        // Dropdown sản phẩm
        public List<SelectListItem> MobilePhones { get; set; }

        // Constructor với danh sách sản phẩm
        public CreateImportViewModel(List<SelectListItem> mobilePhones)
        {
            MobilePhones = mobilePhones;
        }

        // Constructor rỗng
        public CreateImportViewModel()
        {
            MobilePhones = new List<SelectListItem>();
        }
    }

    // ViewModel chỉnh sửa phiếu nhập
    public class EditImportViewModel
    {
        // Dữ liệu phiếu nhập cần edit
        public ImportDto Import { get; set; }

        // Dropdown sản phẩm
        public List<SelectListItem> MobilePhones { get; set; }

        // Id sản phẩm đã chọn trong chi tiết (nếu cần)
        public List<int> SelectedMobilePhoneIds { get; set; } = new List<int>();
    }   }