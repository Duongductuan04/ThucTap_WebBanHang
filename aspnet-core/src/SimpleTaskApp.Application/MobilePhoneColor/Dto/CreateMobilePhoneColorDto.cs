using Abp.AutoMapper;
using Microsoft.AspNetCore.Http;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMapTo(typeof(MobilePhoneColor))]
    public class CreateMobilePhoneColorDto
    {
        public string ColorName { get; set; } // Ví dụ: "Đen", "Trắng", "Xanh"
        public string ColorHex { get; set; }  // Mã màu dạng #000000 (tùy chọn)

        public string ImageUrl { get; set; }  // Nếu ảnh đã tồn tại (đường dẫn)
        public IFormFile ImageFile { get; set; } // Nếu upload ảnh mới qua form
    }
}
