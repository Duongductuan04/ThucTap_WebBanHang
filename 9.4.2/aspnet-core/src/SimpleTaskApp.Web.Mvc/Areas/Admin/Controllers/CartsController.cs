using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.Controllers;
using SimpleTaskApp.Areas.Admin.Models.Carts;
using System.Threading.Tasks;
using SimpleTaskApp.MobilePhones.Dto;
using SimpleTaskApp.MobilePhones;

namespace SimpleTaskApp.Areas.Admin.Controllers
{
    [Area("Admin")]

    [AbpAuthorize]
    public class CartsController : SimpleTaskAppControllerBase
    {
        private readonly ICartAppService _cartAppService;

        public CartsController(ICartAppService cartAppService)
        {
            _cartAppService = cartAppService;
        }


        // =================== INDEX ===================
        public async Task<IActionResult> Index()
        {
            var carts = await _cartAppService.GetAllAsync(new PagedCartResultRequestDto { MaxResultCount = 1000 });
            return View(carts.Items);
        }



        // =================== EDIT MODAL ===================
        public async Task<ActionResult> EditModal(int Id)
        {
            var cart = await _cartAppService.GetAsync(new EntityDto<int>(Id));
           
            return PartialView("_EditModal", new EditCartViewModel
            {
                Cart = cart
            });
        }

        // =================== DELETE ===================
        [HttpPost]
        public async Task<IActionResult> Delete(EntityDto<int> input)
        {
            await _cartAppService.DeleteAsync(input);
            return Json(new { success = true });
        }

     
    }
}
