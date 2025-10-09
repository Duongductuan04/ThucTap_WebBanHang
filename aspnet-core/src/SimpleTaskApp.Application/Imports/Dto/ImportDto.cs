using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMap(typeof(Import))]
    public class ImportDto : IEntityDto<int>
    {
        public int Id { get; set; }

        public string ImportCode { get; set; }
        public string SupplierName { get; set; }
        public string KeeperName { get; set; }
        public string KeeperPhone { get; set; }

        public string Note { get; set; }
        public DateTime CreationTime { get; set; } // thời gian tạo

        // Danh sách chi tiết phiếu nhập
        public List<ImportDetailDto> ImportDetails { get; set; } = new List<ImportDetailDto>();
    }
}
