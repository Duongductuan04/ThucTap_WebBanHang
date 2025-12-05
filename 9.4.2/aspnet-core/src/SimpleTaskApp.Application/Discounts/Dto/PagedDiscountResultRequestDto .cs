using Abp.Application.Services.Dto;

namespace SimpleTaskApp.MobilePhones.Dto
{
    public class PagedDiscountResultRequestDto : PagedResultRequestDto
    {
    public string Keyword { get; set; }

    // Lọc theo trạng thái hoạt động
    public bool? IsActive { get; set; }

    // Lọc theo loại áp dụng
    public int? ApplyType { get; set; } // 0 = toàn bộ, 1 = theo danh mục, 2 = theo sản phẩm

  }
}
