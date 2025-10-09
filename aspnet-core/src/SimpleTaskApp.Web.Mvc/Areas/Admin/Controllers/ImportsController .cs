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

        public ImportsController(
            IImportAppService importAppService,
            IRepository<MobilePhone, int> productRepository)
        {
            _importAppService = importAppService;
            _productRepository = productRepository;
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

            var vm = new CreateImportViewModel
            {
                MobilePhones = mobilePhones
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

            var vm = new EditImportViewModel
            {
                Import = import,
                MobilePhones = mobilePhones,
                SelectedMobilePhoneIds = import.ImportDetails.Select(d => d.MobilePhoneId).ToList()
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
