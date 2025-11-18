using Abp.Application.Services.Dto;
using System;

namespace SimpleTaskApp.MobilePhones.Dto
{
    public class PagedImportResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }          // tìm theo mã phiếu hoặc tên NCC
        public string SupplierName { get; set; }     // lọc theo nhà cung cấp
        public string KeeperName { get; set; }       // lọc theo người nhập
        public DateTime? FromDate { get; set; }      // lọc theo ngày bắt đầu
        public DateTime? ToDate { get; set; }        // lọc theo ngày kết thúc
    }
}
