using Abp.Application.Services.Dto;

namespace SimpleTaskApp.MobilePhones.Dto
{
    public class PagedOrderResultRequestDto : PagedAndSortedResultRequestDto
    {
        // Có thể thêm Keyword để tìm kiếm theo tên nếu muốn
        public long? UserId { get; set; }
        public int? Status { get; set; }
        public string Keyword { get; set; }

    }
}
