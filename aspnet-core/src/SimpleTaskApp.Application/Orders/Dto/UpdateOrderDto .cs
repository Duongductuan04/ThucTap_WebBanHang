using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMapTo(typeof(Order))]
    public class UpdateOrderDto : EntityDto<int>
    {
        public string RecipientName { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientPhone { get; set; }

        public int Status { get; set; }           // 0 = Pending, 1 = Shipping, 2 = Completed, 3 = Cancelled
        public int PaymentMethod { get; set; }    // 0 = Tiền mặt, 1 = Chuyển khoản, 2 = Ví điện tử
        public int ShippingMethod { get; set; }   // 0 = Tiêu chuẩn, 1 = Nhanh, 2 = Siêu tốc

        public int? DiscountId { get; set; }      // Nếu cần thay đổi voucher
        public string Note { get; set; }          // Ghi chú mới
    }
}
