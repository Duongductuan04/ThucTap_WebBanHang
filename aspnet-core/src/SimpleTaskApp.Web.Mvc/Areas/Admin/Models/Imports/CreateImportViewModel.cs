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

    // Danh sách màu của các sản phẩm
    public List<MobilePhoneColorDto> MobilePhoneColors { get; set; }

    // Constructor với danh sách sản phẩm
    public CreateImportViewModel(List<SelectListItem> mobilePhones)
    {
      MobilePhones = mobilePhones;
      MobilePhoneColors = new List<MobilePhoneColorDto>();
    }

    // Constructor rỗng
    public CreateImportViewModel()
    {
      MobilePhones = new List<SelectListItem>();
      MobilePhoneColors = new List<MobilePhoneColorDto>();
    }
  }

  // ViewModel chỉnh sửa phiếu nhập
  public class EditImportViewModel
  {
    // Dữ liệu phiếu nhập cần edit
    public ImportDto Import { get; set; }

    // Dropdown sản phẩm
    public List<SelectListItem> MobilePhones { get; set; }

    // Danh sách màu của các sản phẩm
    public List<MobilePhoneColorDto> MobilePhoneColors { get; set; }

    // Id sản phẩm đã chọn trong chi tiết (nếu cần)
    public List<int> SelectedMobilePhoneIds { get; set; } = new List<int>();

    public EditImportViewModel()
    {
      MobilePhones = new List<SelectListItem>();
      MobilePhoneColors = new List<MobilePhoneColorDto>();
    }
  }

  // DTO màu để JS có thể đọc dễ dàng
  
}
