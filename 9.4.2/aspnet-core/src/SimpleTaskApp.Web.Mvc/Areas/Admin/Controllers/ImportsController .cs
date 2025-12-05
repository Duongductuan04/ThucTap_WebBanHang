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
    private readonly IRepository<Supplier, int> _supplierRepository;

    public ImportsController(
        IImportAppService importAppService,
        IRepository<MobilePhone, int> productRepository,
        IRepository<MobilePhoneColor, int> colorRepository,
        IRepository<Import, int> importRepository,
        IRepository<Supplier, int> supplierRepository)
    {
      _importAppService = importAppService;
      _productRepository = productRepository;
      _colorRepository = colorRepository;
      _importRepository = importRepository;
      _supplierRepository = supplierRepository;
    }

    public async Task<IActionResult> Index()
    {
      var suppliers = await _supplierRepository.GetAll()
          .Select(s => new SelectListItem
          {
            Value = s.Id.ToString(),
            Text = s.SupplierName
          })
          .ToListAsync();

      var keepers = await _importRepository.GetAll()
          .Where(i => !string.IsNullOrEmpty(i.KeeperName))
          .Select(i => i.KeeperName)
          .Distinct()
          .ToListAsync();
      var keepersList = keepers.Select(k => new SelectListItem { Value = k, Text = k }).ToList();

      var mobilePhones = await _productRepository.GetAllListAsync();
      var mobilePhoneItems = mobilePhones
          .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
          .ToList();

      ViewBag.Suppliers = suppliers;
      ViewBag.Keepers = keepersList;
      ViewBag.MobilePhones = mobilePhoneItems;

      return View();
    }

    public async Task<PartialViewResult> CreateModal()
    {
      // Lấy danh sách sản phẩm
      var mobilePhones = (await _productRepository.GetAllListAsync())
          .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
          .ToList();

      // Lấy danh sách màu
      var mobileColors = await _colorRepository.GetAllListAsync();

      // Lấy danh sách nhà cung cấp
      var suppliers = (await _supplierRepository.GetAllListAsync())
          .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.SupplierName })
          .ToList();

      // Tạo ViewModel
      var vm = new CreateImportViewModel
      {
        MobilePhones = mobilePhones,
        MobilePhoneColors = mobileColors.Select(c => new MobilePhoneColorDto
        {
          Id = c.Id,
          MobilePhoneId = c.MobilePhoneId,
          ColorName = c.ColorName
        }).ToList(),
        Suppliers = suppliers
      };

      return PartialView("_CreateModal", vm);
    }

    public async Task<PartialViewResult> EditModal(int importId)
    {
      var import = await _importAppService.GetAsync(new EntityDto<int>(importId));

      // Lấy danh sách sản phẩm
      var mobilePhones = (await _productRepository.GetAllListAsync())
          .Select(p => new SelectListItem
          {
            Value = p.Id.ToString(),
            Text = p.Name,
            Selected = import.ImportDetails.Any(d => d.MobilePhoneId == p.Id)
          })
          .ToList();

      // Lấy danh sách màu
      var mobileColors = await _colorRepository.GetAllListAsync();

      var suppliers = (await _supplierRepository.GetAllListAsync())
    .Select(s => new SelectListItem
    {
      Value = s.Id.ToString(),
      Text = s.SupplierName,
      Selected = import.SupplierId == s.Id  // nếu SupplierId là int
    })
    .ToList();

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
        }).ToList(),
        Suppliers = suppliers // gán dropdown nhà cung cấp
      };

      return PartialView("_EditModal", vm);
    }

    public async Task<PartialViewResult> DetailModal(int importId)
    {
      var import = await _importAppService.GetAsync(new EntityDto<int>(importId));
      return PartialView("_DetailModal", import);
    }

    [HttpGet]
    public async Task<IActionResult> PrintImport(int id)
    {
      var import = await _importAppService.GetAsync(new EntityDto<int>(id));
      if (import == null)
        return NotFound();
      return View("ImportPrint", import);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(EntityDto<int> input)
    {
      await _importAppService.DeleteAsync(input);
      return Json(new { success = true });
    }
  }

}
