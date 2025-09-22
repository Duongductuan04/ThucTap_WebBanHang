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

        // Trang danh sách sản phẩm với các bộ lọc
        public async Task<IActionResult> Index(
            int? categoryId, bool? isNew, bool? isOnSale,
            decimal? minPrice, decimal? maxPrice, string brand,
            int page = 1, int pageSize = 10)

        {
            var request = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (page - 1) * pageSize,
                MaxResultCount = pageSize,
                CategoryId = categoryId,
                IsNew = isNew,
                IsOnSale = isOnSale,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Brand = brand
            };

            var products = await _mobilePhoneAppService.GetAllAsync(request);

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedCategoryName = categoryId != null ? await _mobilePhoneCategoryAppService.GetNameAsync(categoryId.Value) : null;

            ViewBag.IsNewFilter = isNew;
            ViewBag.IsOnSaleFilter = isOnSale;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            // Lấy danh sách brand theo category
            ViewBag.Brands = categoryId != null
                ? await _mobilePhoneAppService.GetBrandsByCategoryAsync(categoryId.Value)
                : new List<string>();


            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = products.TotalCount;

            return View(products.Items);
        }



        public async Task<IActionResult> Detail(int id, int relatedPage = 1, int relatedPageSize = 6)
        {
            var product = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(id));
            if (product == null)
            {
                return NotFound();
            }

            // Lấy sản phẩm liên quan cùng danh mục, bỏ sản phẩm hiện tại
            var request = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (relatedPage - 1) * relatedPageSize,
                MaxResultCount = relatedPageSize  ,
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
        // Hàng mới về
        public async Task<IActionResult> New(int page = 1, int pageSize = 10)
        {
            var request = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (page - 1) * pageSize,
                MaxResultCount = pageSize,
                IsNew = true
            };

            var products = await _mobilePhoneAppService.GetAllAsync(request);

            ViewBag.IsNewFilter = true;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = products.TotalCount;
            ViewBag.SelectedCategoryId = null; // Không có category
            ViewBag.Brands = await _mobilePhoneAppService.GetBrandsByCategoryAsync(null); // hoặc tạo hàm lấy tất cả brand

            return View("Index", products.Items);
        }

        // Hàng khuyến mãi (đang sale)
        public async Task<IActionResult> Promotion(int page = 1, int pageSize = 10)
        {
            var request = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (page - 1) * pageSize,
                MaxResultCount = pageSize,
                IsOnSale = true
            };

            var products = await _mobilePhoneAppService.GetAllAsync(request);

            ViewBag.IsOnSaleFilter = true;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = products.TotalCount;
            ViewBag.SelectedCategoryId = null; // Không có category
            ViewBag.Brands = await _mobilePhoneAppService.GetBrandsByCategoryAsync(null); // hoặc tạo hàm lấy tất cả brand

            return View("Index", products.Items);
        }

        // Tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(query))
                return RedirectToAction("Index");

            var request = new PagedMobilePhoneResultRequestDto
            {
                SkipCount = (page - 1) * pageSize,
                MaxResultCount = pageSize,
                Keyword = query
            };

            var products = await _mobilePhoneAppService.GetAllAsync(request);

            ViewBag.SearchQuery = query;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = products.TotalCount;
            ViewBag.SelectedCategoryId = null; // Không có category
            ViewBag.Brands = await _mobilePhoneAppService.GetBrandsByCategoryAsync(null); // hoặc tạo hàm lấy tất cả brand
            return View("Index", products.Items);
        }

    }
}