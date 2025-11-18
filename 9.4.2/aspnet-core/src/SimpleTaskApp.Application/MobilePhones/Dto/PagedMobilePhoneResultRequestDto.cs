using Abp.Application.Services.Dto;

namespace SimpleTaskApp.MobilePhones.Dto
{
    public class PagedMobilePhoneResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }
        public int? CategoryId { get; set; }      // thêm để lọc theo danh mục
        public bool? IsNew { get; set; }
        public bool? IsOnSale { get; set; }
        // MỚI: filter theo giá và thương hiệu
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Brand { get; set; }
        public string Sort { get; set; }             // thêm sort (priceAsc, priceDesc, new, sale)


    }
}
