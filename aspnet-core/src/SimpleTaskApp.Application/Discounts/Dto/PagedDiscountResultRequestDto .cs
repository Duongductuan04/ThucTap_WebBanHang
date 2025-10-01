using Abp.Application.Services.Dto;

namespace SimpleTaskApp.MobilePhones.Dto
{
    public class PagedDiscountResultRequestDto : PagedResultRequestDto
    {
        public bool? IsActive { get; set; } // Lọc theo trạng thái
    }
}
