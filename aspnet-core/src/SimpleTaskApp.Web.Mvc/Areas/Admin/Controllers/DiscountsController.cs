using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleTaskApp.Areas.Admin.Models.Discounts;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.MobilePhones.Dto;
using SimpleTaskApp.DiscountCategorys.Dto;
using SimpleTaskApp.DiscountProducts.Dto;
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
    [AbpAuthorize(PermissionNames.Pages_Discount)]
    public class DiscountsController : SimpleTaskAppControllerBase
    {
        private readonly IDiscountAppService _discountAppService;
        private readonly IRepository<MobilePhoneCategory, int> _categoryRepository;
        private readonly IRepository<MobilePhone, int> _productRepository;

        public DiscountsController(
            IDiscountAppService discountAppService,
            IRepository<MobilePhoneCategory, int> categoryRepository,
            IRepository<MobilePhone, int> productRepository)
        {
            _discountAppService = discountAppService;
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        // =================== INDEX ===================
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách categories/products trực tiếp
            var categories = (await _categoryRepository.GetAllListAsync())
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            var products = (await _productRepository.GetAllListAsync())
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            ViewBag.Categories = categories;
            ViewBag.Products = products;

            return View();
        }

        // =================== CREATE MODAL ===================
        public async Task<PartialViewResult> CreateModal()
        {
            var categories = (await _categoryRepository.GetAllListAsync())
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            var products = (await _productRepository.GetAllListAsync())
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var vm = new CreateDiscountViewModel
            {
                Categories = categories,
                Products = products
            };

            return PartialView("_CreateModal", vm);
        }

        // =================== EDIT MODAL ===================
        public async Task<PartialViewResult> EditModal(int discountId)
        {
            var discount = await _discountAppService.GetAsync(new EntityDto<int>(discountId));

            var categories = (await _categoryRepository.GetAllListAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = discount.Categories.Any(dc => dc.CategoryId == c.Id)
                })
                .ToList();

            var products = (await _productRepository.GetAllListAsync())
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Selected = discount.Products.Any(dp => dp.MobilePhoneId == p.Id)
                })
                .ToList();

            var vm = new EditDiscountViewModel
            {
                Discount = discount,
                Categories = categories,
                Products = products,
                SelectedCategoryIds = discount.Categories.Select(dc => dc.CategoryId).ToList(),
                SelectedProductIds = discount.Products.Select(dp => dp.MobilePhoneId).ToList()
            };

            return PartialView("_EditModal", vm);
        }

        // =================== DETAIL MODAL ===================
        public async Task<PartialViewResult> DetailModal(int discountId)
        {
            var discount = await _discountAppService.GetAsync(new EntityDto<int>(discountId));
            return PartialView("_DetailModal", discount);
        }

        // =================== DELETE ===================
        [HttpPost]
        public async Task<IActionResult> Delete(EntityDto<int> input)
        {
            await _discountAppService.DeleteAsync(input);
            return Json(new { success = true });
        }
    }
}
