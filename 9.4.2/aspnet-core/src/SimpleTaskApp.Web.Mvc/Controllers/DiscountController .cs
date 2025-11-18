using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SimpleTaskApp.MobilePhones;
using Abp.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using SimpleTaskApp.MobilePhones.Dto;

namespace SimpleTaskApp.Web.Controllers
{
    public class DiscountController : AbpController
    {
        private readonly IOrderAppService _orderAppService;

        public DiscountController(IOrderAppService orderAppService)
        {
            _orderAppService = orderAppService;
        }

        // Action trả về toàn bộ trang (ví dụ: trang Checkout)
        public async Task<IActionResult> Index(List<OrderItemDto> cartItems, decimal cartTotalAmount)
        {
            var discounts = await _orderAppService.GetAvailableDiscountsAsync(cartItems, cartTotalAmount);
            return View(discounts);
        }

        // Action trả về PartialView để nhúng vào trang khác (ví dụ: modal)
        [HttpPost]
        public async Task<PartialViewResult> AvailableDiscountsPartial(
            [FromBody] List<OrderItemDto> cartItems,
            [FromQuery] decimal totalAmount)
        {
            // Gọi service lọc voucher
            var discounts = await _orderAppService.GetAvailableDiscountsAsync(cartItems, totalAmount);
            return PartialView("_AvailableDiscountsPartial", discounts);
        }
    }
}
