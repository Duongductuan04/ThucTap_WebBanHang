using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleTaskApp.MobilePhones
{
  [Table("AppSuppliers")]
  public class Supplier : Entity, IHasCreationTime
  {
    public string SupplierCode { get; set; }      // Mã NCC
    public string SupplierName { get; set; }      // Tên NCC

    public string Phone { get; set; }             // SĐT NCC
    public string Email { get; set; }             // Email NCC
    public string Address { get; set; }           // Địa chỉ

    public string TaxCode { get; set; }           // Mã số thuế

    public string Note { get; set; }              // Ghi chú

    // 👉 Đổi từ bool sang int
    // 1 = hoạt động, 0 = ngừng
    public int IsActive { get; set; } = 1;

    public DateTime CreationTime { get; set; } = Clock.Now;
  }
}
