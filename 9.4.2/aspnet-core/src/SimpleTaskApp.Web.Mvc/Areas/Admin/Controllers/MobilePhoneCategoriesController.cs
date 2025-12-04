using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.Controllers;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.MobilePhones.Dto;
using System.Threading.Tasks;
using SimpleTaskApp.Areas.Admin.Models.MobilePhoneCategories;

namespace SimpleTaskApp.Areas.Admin.Controllers
{
  [Area("Admin")]
  [AbpAuthorize(PermissionNames.Pages_MobilePhoneCategory)]
  public class MobilePhoneCategoriesController : SimpleTaskAppControllerBase
  {
    private readonly IMobilePhoneCategoryAppService _categoryAppService;

    public MobilePhoneCategoriesController(IMobilePhoneCategoryAppService categoryAppService)
    {
      _categoryAppService = categoryAppService;
    }

    // =================== INDEX ===================
    public IActionResult Index()
    {
      return View();
    }

    // =================== CREATE MODAL ===================
    public PartialViewResult CreateModal()
    {
      var vm = new MobilePhoneCategoryViewModel();
      return PartialView("_CreateModal", vm);
    }

    // =================== EDIT MODAL ===================
    public async Task<PartialViewResult> EditModal(int id)
    {
      var category = await _categoryAppService.GetAsync(new EntityDto<int>(id));

      var vm = new MobilePhoneCategoryViewModel
      {
        Category = category
      };

      return PartialView("_EditModal", vm);
    }

   
    // =================== DELETE ===================
    [HttpPost]
    public async Task<IActionResult> Delete(EntityDto<int> input)
    {
      await _categoryAppService.DeleteAsync(input);
      return Json(new { success = true });
    }
  }
}
