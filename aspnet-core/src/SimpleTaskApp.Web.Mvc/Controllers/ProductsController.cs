using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.MobilePhones.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.Web.Controllers
{
    [AbpAuthorize] // THÊM AUTHORIZATION Ở ĐÂY
    public class ProductsController : AbpController
    {
        private readonly IMobilePhoneAppService _mobilePhoneAppService;
        private readonly IMobilePhoneCategoryAppService _mobilePhoneCategoryAppService;

        public ProductsController(IMobilePhoneAppService mobilePhoneAppService, IMobilePhoneCategoryAppService mobilePhoneCategoryAppService)
        {
            _mobilePhoneAppService = mobilePhoneAppService;
            _mobilePhoneCategoryAppService = mobilePhoneCategoryAppService;
        }

        // Trang danh sách sản phẩm với bộ lọc, sắp xếp, tìm kiếm
        public async Task<IActionResult> Index(
            int? categoryId, string sort, string brand, string query,
            int page = 1, int pageSize = 20 )
        {
            // Xác định filter IsNew, IsOnSale dựa vào sort
            bool? isNew = null;
            bool? isOnSale = null;

            switch (sort)
            {
                case "new":
                    isNew = true;
                    break;
                case "sale":
                    isOnSale = true;
                    break;
                case "priceAsc":
                case "priceDesc":
                    // chỉ sort giá
                    break;
            }

            var request = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (page - 1) * pageSize,
                MaxResultCount = pageSize,
                CategoryId = categoryId,
                Brand = brand,
                IsNew = isNew,
                IsOnSale = isOnSale,
                Keyword = query,
                Sort = sort // cần thêm Sort trong DTO để service xử lý
            };

            var products = await _mobilePhoneAppService.GetAllAsync(request);

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedCategoryName = categoryId != null ? await _mobilePhoneCategoryAppService.GetNameAsync(categoryId.Value) : null;
            ViewBag.SelectedBrand = brand;
            ViewBag.SearchQuery = query;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = products.TotalCount;
            ViewBag.Brands = categoryId != null
                ? await _mobilePhoneAppService.GetBrandsByCategoryAsync(categoryId.Value)
                : new List<string>();

            return View(products.Items);
        }

        // Chi tiết sản phẩm
        public async Task<IActionResult> Detail(int id, int relatedPage = 1, int relatedPageSize = 8)
        {
            var product = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(id));
            if (product == null) return NotFound();

            var request = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (relatedPage - 1) * relatedPageSize,
                MaxResultCount = relatedPageSize,
                CategoryId = product.CategoryId
            };

            var relatedProductsAll = await _mobilePhoneAppService.GetAllAsync(request);
            var relatedProducts = relatedProductsAll.Items.Where(p => p.Id != product.Id).ToList();
             var colors = await _mobilePhoneAppService.GetColorsByMobilePhoneIdAsync(id);

      ViewBag.RelatedProducts = relatedProducts;
            ViewBag.RelatedTotalCount = relatedProductsAll.TotalCount - 1;
            ViewBag.RelatedPage = relatedPage;
            ViewBag.RelatedPageSize = relatedPageSize;
            ViewBag.Colors = colors;

      return View(product);
        }
    }
}
