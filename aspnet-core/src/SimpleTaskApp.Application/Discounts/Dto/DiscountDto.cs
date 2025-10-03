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
    
     // THÊM PROPERTY NÀY - đây là nguyên nhân lỗi
    public string ApplyScopeInfo { get; set; }

        // Property helper để hiển thị thông tin
        public string DiscountInfo
        {
            get
            {
                if (Percentage.HasValue && Percentage.Value > 0)
                    return $"Giảm {Percentage}%";
                else if (Amount.HasValue && Amount.Value > 0)
                    return $"Giảm {Amount.Value.ToString("N0")} đ";
                return "Không có giảm giá";
            }
        }

        public string ConditionInfo
        {
            get
            {
                return $"Đơn tối thiểu: {MinOrderValue.ToString("N0")} đ";
            }
        }

        public string ValidityInfo
        {
            get
            {
                return $"HSD: {StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}";
            }
        }

        // Property helper để hiển thị trạng thái
        public string StatusInfo
        {
            get
            {
                if (MaxUsage > 0)
                    return $"Đã dùng: {CurrentUsage}/{MaxUsage}";
                return "Không giới hạn";
            }
        }
    }
}