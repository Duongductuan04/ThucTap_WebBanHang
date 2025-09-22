using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMap(typeof(OrderDetail))]
    public class OrderDetailDto : EntityDto<int>
    {
        public int OrderId { get; set; }
        public int MobilePhoneId { get; set; }
        public string MobilePhoneName { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ItemTotal => Quantity * UnitPrice;
        public DateTime CreationTime { get; set; }
    }
}