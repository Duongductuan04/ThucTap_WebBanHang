    using Abp.Application.Services.Dto;
    using Abp.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleTaskApp.Authorization;
    using SimpleTaskApp.Controllers;
    using SimpleTaskApp.Areas.Admin.Models.Orders;
    using SimpleTaskApp.MobilePhones;
    using SimpleTaskApp.MobilePhones.Dto;
    using System.Threading.Tasks;
    using Abp.Authorization;

    namespace SimpleTaskApp.Areas.Admin.Controllers
    {
        [Area("Admin")]
        [AbpAuthorize]
        public class OrdersController : SimpleTaskAppControllerBase
        {
            private readonly IOrderAppService _orderAppService;

            public OrdersController(IOrderAppService orderAppService)
            {
                _orderAppService = orderAppService;
            }

            // =================== INDEX ===================
            public async Task<IActionResult> Index()
            {
                var orders = await _orderAppService.GetAllAsync(new PagedOrderResultRequestDto { MaxResultCount = 1000 });
                return View(orders.Items);
            }

            // =================== DETAIL MODAL ===================
            public async Task<PartialViewResult> DetailModal(int Id)
            {
                var order = await _orderAppService.GetAsync(new EntityDto<int>(Id));
                return PartialView("_DetailModal", order);
            }

            // =================== EDIT MODAL ===================
            public async Task<ActionResult> EditModal(int Id)
            {
                var  order = await _orderAppService.GetAsync(new EntityDto<int>(Id));

                return PartialView("_EditModal", new EditOrderViewModel
                {
                    Order = order
                });
            }

            // =================== DELETE ===================
            [HttpPost]
            public async Task<IActionResult> Delete(EntityDto<int> input)
            {
                await _orderAppService.DeleteAsync(input);
                return Json(new { success = true });
            }
        }
    }
