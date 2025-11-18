using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.Controllers;
using SimpleTaskApp.MultiTenancy;

namespace SimpleTaskApp.Areas.Admin.Controllers
{
    [Area("Admin")]

    [AbpMvcAuthorize(PermissionNames.Pages_Tenants)]
    public class TenantsController : SimpleTaskAppControllerBase
    {
        private readonly ITenantAppService _tenantAppService;

        public TenantsController(ITenantAppService tenantAppService)
        {
            _tenantAppService = tenantAppService;
        }

        public ActionResult Index() => View();

        public async Task<ActionResult> EditModal(int tenantId)
        {
            var tenantDto = await _tenantAppService.GetAsync(new EntityDto(tenantId));
            return PartialView("_EditModal", tenantDto);
        }
    }
}
