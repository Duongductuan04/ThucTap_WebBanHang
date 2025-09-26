using Abp.AspNetCore.Mvc;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using Abp.AspNetCore.Mvc.Authorization;
using SimpleTaskApp.Authorization;

namespace SimpleTaskApp.Web.Controllers
{
    [Authorize]
    [AbpMvcAuthorize]

    public class HomeController : AbpController
    {
        private readonly IMobilePhoneAppService _mobilePhoneAppService;

        public HomeController(IMobilePhoneAppService mobilePhoneAppService)
        {
            _mobilePhoneAppService = mobilePhoneAppService;
        }

        // Trang chủ - nhiều nhóm sản phẩm
        public async Task<IActionResult> Index(
            int newPage = 1, int promotionPage = 1,
            int pageSize = 8)
        {
            // 1. Hàng mới
            var newRequest = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (newPage - 1) * pageSize,
                MaxResultCount = pageSize,
                IsNew = true
            };
            var newProductsResult = await _mobilePhoneAppService.GetAllAsync(newRequest);
            ViewBag.NewProducts = newProductsResult.Items;
            ViewBag.NewTotalCount = newProductsResult.TotalCount;
            ViewBag.NewPage = newPage;

            // 2. Hàng khuyến mãi
            var promoRequest = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (promotionPage - 1) * pageSize,
                MaxResultCount = pageSize,
                IsOnSale = true
            };
            var promoProductsResult = await _mobilePhoneAppService.GetAllAsync(promoRequest);
            ViewBag.PromotionProducts = promoProductsResult.Items;
            ViewBag.PromotionTotalCount = promoProductsResult.TotalCount;
            ViewBag.PromotionPage = promotionPage;

            return View();
        }

        // Chi tiết sản phẩm + sản phẩm liên quan có phân trang
        public async Task<IActionResult> Detail(int id, int relatedPage = 1, int relatedPageSize = 8)
        {
            var product = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(id));
            if (product == null)
                return NotFound();

            // Lấy sản phẩm liên quan trong cùng danh mục, trừ sản phẩm hiện tại
            var request = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (relatedPage - 1) * relatedPageSize,
                MaxResultCount = relatedPageSize,
                CategoryId = product.CategoryId
            };

            var relatedProductsAll = await _mobilePhoneAppService.GetAllAsync(request);
            var relatedProducts = relatedProductsAll.Items.Where(p => p.Id != product.Id).ToList();

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.RelatedTotalCount = relatedProductsAll.TotalCount - 1; // trừ sản phẩm hiện tại
            ViewBag.RelatedPage = relatedPage;
            ViewBag.RelatedPageSize = relatedPageSize;

            return View(product);
        }
    }
}
