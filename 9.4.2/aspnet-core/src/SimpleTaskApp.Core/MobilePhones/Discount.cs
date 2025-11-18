using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;

namespace SimpleTaskApp.MobilePhones
{
    [Table("AppDiscounts")]
    public class Discount : Entity, IHasCreationTime
    {
        public string Name { get; set; }          // Tên voucher
        public string Code { get; set; }          // Mã giảm giá (unique)

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Percentage { get; set; }  // % giảm

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }      // Giảm số tiền

        public decimal MinOrderValue { get; set; } = 0; // Đơn hàng tối thiểu

        /// <summary>
        /// 0 = toàn bộ đơn hàng
        /// 1 = theo danh mục
        /// 2 = theo sản phẩm
        /// </summary>
        public int ApplyType { get; set; }

        // Giới hạn lượt sử dụng
        public int MaxUsage { get; set; } = 0; // 0 = không giới hạn
        public int CurrentUsage { get; set; } = 0; // đã sử dụng bao nhiêu lần

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Ngày tạo (tự động gán khi thêm mới)
        public DateTime CreationTime { get; set; } = Clock.Now;
    }
}
