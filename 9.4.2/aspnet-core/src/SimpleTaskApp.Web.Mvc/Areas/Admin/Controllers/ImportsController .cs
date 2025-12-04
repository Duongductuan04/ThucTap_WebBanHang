using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleTaskApp.Areas.Admin.Models.Imports;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using SimpleTaskApp.Controllers;
using Abp.Authorization;
using SimpleTaskApp.Authorization;
using Microsoft.EntityFrameworkCore;

namespace SimpleTaskApp.Areas.Admin.Controllers
{
  [Area("Admin")]
  [AbpAuthorize(PermissionNames.Pages_Import)]
  public class ImportsController : SimpleTaskAppControllerBase
  {
    private readonly IImportAppService _importAppService;
    private readonly IRepository<MobilePhone, int> _productRepository;
    private readonly IRepository<MobilePhoneColor, int> _colorRepository;
    private readonly IRepository<Import, int> _importRepository;

    public ImportsController(
        IImportAppService importAppService,
        IRepository<MobilePhone, int> productRepository,
        IRepository<MobilePhoneColor, int> colorRepository,
    IRepository<Import, int> importRepository) // <-- interface chuẩn

    {
      _importAppService = importAppService;
      _productRepository = productRepository;
      _colorRepository = colorRepository;
      _importRepository = importRepository; // <-- gán giá trị

    }

    // =================== INDEX ===================
    public async Task<IActionResult> Index()
    {
      // Lấy danh sách nhà cung cấp từ repository Import
      // Lấy nhà cung cấp từ bảng Import
      var suppliersData = await _importRepository.GetAll()
          .Where(s => !string.IsNullOrEmpty(s.SupplierName))
          .Select(s => s.SupplierName)
          .Distinct()
          .ToListAsync(); // EF Core async vẫn được vì đây là entity / primitive

      var suppliers = suppliersData
          .Select(s => new SelectListItem
          {
            Value = s,
            Text = s
          })
          .ToList();

      // Tương tự cho Keepers
      var keepersData = await _importRepository.GetAll()
          .Where(k => !string.IsNullOrEmpty(k.KeeperName))
          .Select(k => k.KeeperName)
          .Distinct()
          .ToListAsync();

      var keepers = keepersData
          .Select(k => new SelectListItem
          {
            Value = k,
            Text = k
          })
          .ToList();

      // Nếu cần mobile phones
      var mobilePhones = await _productRepository.GetAllListAsync();
      var mobilePhoneItems = mobilePhones
          .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
          .ToList();

      ViewBag.Suppliers = suppliers;
      ViewBag.Keepers = keepers;
      ViewBag.MobilePhones = mobilePhoneItems;

      return View();
    }


    // =================== CREATE MODAL ===================
    public async Task<PartialViewResult> CreateModal()
    {
      var mobilePhones = (await _productRepository.GetAllListAsync())
          .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
          .ToList();

      var mobileColors = await _colorRepository.GetAllListAsync();

      var vm = new CreateImportViewModel
      {
        MobilePhones = mobilePhones,
        MobilePhoneColors = mobileColors.Select(c => new MobilePhoneColorDto
        {
          Id = c.Id,
          MobilePhoneId = c.MobilePhoneId,
          ColorName = c.ColorName
        }).ToList()
      };

      return PartialView("_CreateModal", vm);
    }

    // =================== EDIT MODAL ===================
    public async Task<PartialViewResult> EditModal(int importId)
    {
      var import = await _importAppService.GetAsync(new EntityDto<int>(importId));

      var mobilePhones = (await _productRepository.GetAllListAsync())
          .Select(p => new SelectListItem
          {
            Value = p.Id.ToString(),
            Text = p.Name,
            Selected = import.ImportDetails.Any(d => d.MobilePhoneId == p.Id)
          })
          .ToList();

      var mobileColors = await _colorRepository.GetAllListAsync();

      var vm = new EditImportViewModel
      {
        Import = import,
        MobilePhones = mobilePhones,
        SelectedMobilePhoneIds = import.ImportDetails.Select(d => d.MobilePhoneId).ToList(),
        MobilePhoneColors = mobileColors.Select(c => new MobilePhoneColorDto
        {
          Id = c.Id,
          MobilePhoneId = c.MobilePhoneId,
          ColorName = c.ColorName
        }).ToList()
      };

      return PartialView("_EditModal", vm);
    }

    // =================== DETAIL MODAL ===================
    public async Task<PartialViewResult> DetailModal(int importId)
    {
      var import = await _importAppService.GetAsync(new EntityDto<int>(importId));
      return PartialView("_DetailModal", import);
    }
    [HttpGet]
    public async Task<IActionResult> PrintImport(int id)
    {
      // Lấy phiếu nhập theo Id
      var import = await _importAppService.GetAsync(new EntityDto<int>(id));
      if (import == null)
        return NotFound();

      // Truyền sang view chuyên dụng để in
      return View("ImportPrint", import);
    }
    // =================== DELETE ===================
    [HttpPost]
    public async Task<IActionResult> Delete(EntityDto<int> input)
    {
      await _importAppService.DeleteAsync(input);
      return Json(new { success = true });
    }
  }
}
