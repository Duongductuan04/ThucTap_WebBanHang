using SimpleTaskApp.MobilePhones.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SimpleTaskApp.Areas.Admin.Models.Imports
{
  // ViewModel tạo phiếu nhập
  public class CreateImportViewModel
  {
    // Dropdown sản phẩm
    public List<SelectListItem> MobilePhones { get; set; } = new List<SelectListItem>();

    // Danh sách màu của các sản phẩm
    public List<MobilePhoneColorDto> MobilePhoneColors { get; set; } = new List<MobilePhoneColorDto>();

    // Constructor có thể truyền danh sách sản phẩm
    public List<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();

    public CreateImportViewModel(List<SelectListItem> mobilePhones)
    {
      MobilePhones = mobilePhones ?? new List<SelectListItem>();
    }

    // Constructor rỗng
    public CreateImportViewModel() { }
  }

  // ViewModel chỉnh sửa phiếu nhập
  public class EditImportViewModel
  {
    // Dữ liệu phiếu nhập cần edit
    public ImportDto Import { get; set; }

    // Dropdown sản phẩm
    public List<SelectListItem> MobilePhones { get; set; } = new List<SelectListItem>();

    // Danh sách màu của các sản phẩm
    public List<MobilePhoneColorDto> MobilePhoneColors { get; set; } = new List<MobilePhoneColorDto>();
    public List<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();

    // Id sản phẩm đã chọn trong chi tiết
    public List<int> SelectedMobilePhoneIds { get; set; } = new List<int>();

    public EditImportViewModel() { }
  }
}
