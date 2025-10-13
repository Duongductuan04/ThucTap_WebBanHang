using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMap(typeof(MobilePhoneColor))]
    public class MobilePhoneColorDto : EntityDto<int>
    {
        public int MobilePhoneId { get; set; }   // Liên kết về sản phẩm
        public string ColorName { get; set; }    // Tên màu (Đen, Trắng,...)
        public string ColorHex { get; set; }     // Mã màu (VD: #000000)
        public string ImageUrl { get; set; }     // Ảnh đại diện của màu
        public string MobilePhoneName { get; set; }

    }
}
