using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppImports")]
    public class Import : Entity, IHasCreationTime
    {
        public string ImportCode { get; set; } // Mã phiếu nhập
        public string SupplierName { get; set; } // Tên nhà cung cấp
        public string Note { get; set; } // Ghi chú

        // 👇 Thông tin thủ kho (người thực hiện nhập hàng)
        public string KeeperName { get; set; }
        public string KeeperPhone { get; set; }

        public DateTime CreationTime { get; set; } = Clock.Now;

        // Danh sách chi tiết phiếu nhập
        public virtual ICollection<ImportDetail> ImportDetails { get; set; }
    }
}
