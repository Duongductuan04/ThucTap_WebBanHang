using Abp.Application.Services.Dto;

namespace SimpleTaskApp.MobilePhones.Dto
{
    public class PagedMobilePhoneColorResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }          // tìm theo tên màu
        public int? MobilePhoneId { get; set; }      // lọc theo điện thoại
    }
}
