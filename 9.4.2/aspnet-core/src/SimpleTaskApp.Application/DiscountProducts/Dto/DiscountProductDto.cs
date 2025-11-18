using Abp.AutoMapper;
using SimpleTaskApp.MobilePhones;
using System;

namespace SimpleTaskApp.DiscountProducts.Dto
{
    [AutoMapTo(typeof(DiscountProduct))]
    public class DiscountProductDto
    {
        public int Id { get; set; }                // Id của bản ghi
        public int MobilePhoneId { get; set; }     // Id điện thoại
        public string MobilePhoneName { get; set; }// Tên điện thoại
        public int DiscountId { get; set; }        // Id của Discount liên quan
    }
}
