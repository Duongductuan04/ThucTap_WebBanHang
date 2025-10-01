using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using SimpleTaskApp.MobilePhones;
using System;
using System.Collections.Generic;
using SimpleTaskApp.DiscountCategorys.Dto;
using SimpleTaskApp.DiscountProducts.Dto;
namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMapFrom(typeof(Discount))]
    public class DiscountDto : EntityDto<int>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public decimal MinOrderValue { get; set; } = 0;
        public int ApplyType { get; set; }
        public int MaxUsage { get; set; } = 0;
        public int CurrentUsage { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreationTime { get; set; }

        public List<DiscountCategoryDto> Categories { get; set; } = new List<DiscountCategoryDto>();
        public List<DiscountProductDto> Products { get; set; } = new List<DiscountProductDto>();

        public int TotalCategories => Categories?.Count ?? 0;
        public int TotalProducts => Products?.Count ?? 0;
    }

}
