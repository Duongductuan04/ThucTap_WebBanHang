using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.Controllers;
using SimpleTaskApp.Suppliers;
using SimpleTaskApp.Suppliers.Dto;
using System.Threading.Tasks;
using SimpleTaskApp.Areas.Admin.Models.Suppliers;

namespace SimpleTaskApp.Areas.Admin.Controllers
{
  [Area("Admin")]
  [AbpAuthorize(PermissionNames.Pages_Suppliers)]
  public class SuppliersController : SimpleTaskAppControllerBase
  {
    private readonly ISupplierAppService _supplierAppService;

    public SuppliersController(ISupplierAppService supplierAppService)
    {
      _supplierAppService = supplierAppService;
    }

    // =================== INDEX ===================
    public IActionResult Index()
    {
      return View();
    }

    // =================== CREATE MODAL ===================
    public PartialViewResult CreateModal()
    {
      var vm = new SupplierViewModel();
      return PartialView("_CreateModal", vm);
    }

    // =================== EDIT MODAL ===================
    public async Task<PartialViewResult> EditModal(int id)
    {
      var supplier = await _supplierAppService.GetByIdAsync(id);

      var vm = new SupplierViewModel
      {
        Supplier = supplier
      };

      return PartialView("_EditModal", vm);
    }
    // =================== DETAIL MODAL ===================
    public async Task<PartialViewResult> DetailModal(int id)
    {
      // Lấy dữ liệu nhà cung cấp theo id
      var supplier = await _supplierAppService.GetByIdAsync(id);

      // Tạo ViewModel nếu cần, hoặc truyền trực tiếp DTO
      var vm = new SupplierViewModel
      {
        Supplier = supplier
      };

      // Trả về PartialView _DetailModal.cshtml
      return PartialView("_DetailModal", vm);
    }
    // =================== DELETE ===================
    [HttpPost]
    [AbpAuthorize(PermissionNames.Pages_Suppliers_Delete)]
    public async Task<IActionResult> Delete(EntityDto<int> input)
    {
      await _supplierAppService.DeleteAsync(input.Id);
      return Json(new { success = true });
    }
  }
}
