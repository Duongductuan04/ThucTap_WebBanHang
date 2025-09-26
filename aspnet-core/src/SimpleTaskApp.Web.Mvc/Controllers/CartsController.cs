using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.MobilePhones.Dto;
using SimpleTaskApp.MobilePhones;
using System.Threading.Tasks;
using SimpleTaskApp.Authorization.Users;
using Abp.Runtime.Session;

using Microsoft.AspNetCore.Antiforgery;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using System;

namespace SimpleTaskApp.Web.Controllers
{
    [AbpAuthorize] // Chỉ user/admin có quyền xem

    public class CartsController : AbpController
    {   
        private readonly ICartAppService _cartAppService;
        private readonly IAntiforgery _antiforgery;
        private readonly IAbpSession _abpSession; // thêm


        public CartsController(ICartAppService cartAppService, IAntiforgery antiforgery, IAbpSession abpSession)
        {
            _cartAppService = cartAppService;
            _antiforgery = antiforgery;
            _abpSession = abpSession; // ✅ gán đúng

        }

        // Trang danh sách giỏ hàng
        public async Task<IActionResult> Index()
        {    // Kiểm tra đăng nhập
            if (!_abpSession.UserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            var carts = await _cartAppService.GetAllAsync(new PagedCartResultRequestDto
            {
                UserId = _abpSession.UserId.Value, MaxResultCount = 100 });
            return View(carts.Items);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddToCart(int mobilePhoneId, int quantity)
        {
            if (!_abpSession.UserId.HasValue)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập trước!" });
            }

            if (mobilePhoneId <= 0 || quantity <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            var cartDto = await _cartAppService.AddToCartAsync(new CreateCartDto
            {
                MobilePhoneId = mobilePhoneId,
                Quantity = quantity,
                UserId = _abpSession.UserId.Value
            });

            if (cartDto == null)
            {
                return Json(new { success = false, message = "Thêm vào giỏ hàng thất bại!" });
            }

            return Json(new
            {
                success = true,
                message = $"Đã thêm \"{cartDto.Name}\" (x{cartDto.Quantity}) vào giỏ hàng!",
                cart = cartDto
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateQuantityAjax(int cartId, int quantity)
        {
            try
            {
                await _cartAppService.UpdateQuantityAsync(cartId, quantity);
                return Json(new { success = true, message = "Cập nhật số lượng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // Delete cart item - THÊM PHƯƠNG THỨC MỚI
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> DeleteCartItem(int cartId)
        {
            await _cartAppService.DeleteAsync(new EntityDto<int>(cartId));
            return RedirectToAction("Index");
        }

    }
}