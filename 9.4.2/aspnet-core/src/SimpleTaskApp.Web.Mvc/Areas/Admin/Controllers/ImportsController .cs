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

namespace SimpleTaskApp.Areas.Admin.Controllers
{
  [Area("Admin")]
  [AbpAuthorize(PermissionNames.Pages_Import)]
  public class ImportsController : SimpleTaskAppControllerBase
  {
    private readonly IImportAppService _importAppService;
    private readonly IRepository<MobilePhone, int> _productRepository;
    private readonly IRepository<MobilePhoneColor, int> _colorRepository;

    public ImportsController(
        IImportAppService importAppService,
        IRepository<MobilePhone, int> productRepository,
        IRepository<MobilePhoneColor, int> colorRepository)
    {
      _importAppService = importAppService;
      _productRepository = productRepository;
      _colorRepository = colorRepository;
    }

    // =================== INDEX ===================
    public async Task<IActionResult> Index()
    {
      var mobilePhones = (await _productRepository.GetAllListAsync())
          .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
          .ToList();

      ViewBag.MobilePhones = mobilePhones;

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

    // =================== DELETE ===================
    [HttpPost]
    public async Task<IActionResult> Delete(EntityDto<int> input)
    {
      await _importAppService.DeleteAsync(input);
      return Json(new { success = true });
    }
  }
}
