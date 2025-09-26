using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.Controllers;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.MobilePhones.Dto;
using SimpleTaskApp.Areas.Admin.Models.MobilePhones;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace SimpleTaskApp.Areas.Admin.Controllers
{
    [Area("Admin")]

    [AbpAuthorize(PermissionNames.Pages_MobilePhone)]
    public class MobilePhonesController : SimpleTaskAppControllerBase
    {
        private readonly IMobilePhoneAppService _mobilePhoneAppService;
        private readonly IMobilePhoneCategoryAppService _categoryAppService;
        private readonly IWebHostEnvironment _env;
        private readonly string _imagePath;
        //private readonly IConfiguration _configuration;
        public MobilePhonesController(
            IMobilePhoneAppService mobilePhoneAppService,
            MobilePhoneCategoryAppService categoryAppService,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            _mobilePhoneAppService = mobilePhoneAppService;
            _categoryAppService = categoryAppService;
            _env = env;
            _imagePath = configuration["UploadSettings:ImagePath"];
        }

        // Trang chính
        public async Task<IActionResult> Index()
        {
            var categories = (await _categoryAppService.GetAllAsync(new PagedMobilePhoneCategoryResultRequestDto { MaxResultCount = 1000 }))
                .Items
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            ViewBag.Categories = categories;
            return View();
        }

        // =================== CREATE MODAL ===================
        public async Task<ActionResult> CreateModal()
        {
            var categoryItems = (await _categoryAppService.GetAllAsync(new PagedMobilePhoneCategoryResultRequestDto { MaxResultCount = 1000 }))
                .Items
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            return PartialView("_CreateModal", new CreateMobilePhoneViewModel
            {
                Categories = categoryItems
            });
        }

    

        // =================== EDIT MODAL ===================
        public async Task<ActionResult> EditModal(int mobilePhoneId)
        {
            var phone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(mobilePhoneId));

            var categoryItems = (await _categoryAppService.GetAllAsync(new PagedMobilePhoneCategoryResultRequestDto { MaxResultCount = 1000 }))
                .Items
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == phone.CategoryId
                })
                .ToList();

            return PartialView("_EditModal", new EditMobilePhoneViewModel
            {
                MobilePhone = phone,
                Categories = categoryItems
            });
        }

    

        // =================== DELETE ===================
        [HttpPost]
        public async Task<IActionResult> Delete(EntityDto<int> input)
        {
            await _mobilePhoneAppService.DeleteAsync(input);
            return Json(new { success = true });
        }

        // =================== DETAIL ===================

        public async Task<PartialViewResult> DetailModal(int mobilePhoneId)
        {
            var phone = await _mobilePhoneAppService.GetAsync(new EntityDto<int>(mobilePhoneId));
            return PartialView("_DetailModal", phone);
        }


        #region Upload File 
        
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Ok(new { fileUrl = "" }); // Trả về fileUrl rỗng thay vì BadRequest

            if (!Directory.Exists(_imagePath))
                Directory.CreateDirectory(_imagePath);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_imagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về URL web
            var fileUrl = "/uploads/" + fileName;

            return Ok(new { fileUrl });
        }

        #endregion
    }



}
