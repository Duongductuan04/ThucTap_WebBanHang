using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Microsoft.AspNetCore.Http;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMap(typeof(MobilePhoneColor))]
    public class UpdateMobilePhoneColorDto : EntityDto<int>
    {
        public int MobilePhoneId { get; set; }          // Liên kết tới điện thoại

        public string ColorName { get; set; }           // Ví dụ: "Đen", "Trắng"
        public string ColorHex { get; set; }            // Mã màu dạng #000000

        public string ImageUrl { get; set; }            // Ảnh hiện tại (nếu đã có)
        public IFormFile ImageFile { get; set; }        // Ảnh mới (nếu upload lại)
    }
}
