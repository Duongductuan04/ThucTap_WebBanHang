using Abp.AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMapTo(typeof(Import))]
    public class CreateImportDto
    {
        public string ImportCode { get; set; }

        [Required]
        public string SupplierName { get; set; }

        public string Note { get; set; }
        public string KeeperName { get; set; }
        public string KeeperPhone { get; set; }

        public List<CreateImportDetailDto> ImportDetails { get; set; } = new List<CreateImportDetailDto>();
    }
    // ====== DTO cập nhật Import ======
    public class UpdateImportDto : CreateImportDto
    {
        public int Id { get; set; } // Id của phiếu nhập
    }
}
