using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using SimpleTaskApp.MobilePhones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Suppliers.Dto
{
  // ===========================
  // DTO trả về dữ liệu Supplier
  // ===========================
  [AutoMapTo(typeof(Supplier))]
  public class SupplierDto : EntityDto<int>
  {
    public string SupplierCode { get; set; }
    public string SupplierName { get; set; }

    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }

    public string TaxCode { get; set; }
    public string Note { get; set; }

    public int IsActive { get; set; }
    public DateTime CreationTime { get; set; }
  }

  // ===========================
  // DTO tạo mới Supplier
  // ===========================
  [AutoMapTo(typeof(Supplier))]
  public class CreateSupplierDto
  {
    [Required]
    public string SupplierCode { get; set; }

    [Required]
    public string SupplierName { get; set; }

    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }

    public string TaxCode { get; set; }
    public string Note { get; set; }

    public int IsActive { get; set; } = 1;
  }

  // ===========================
  // DTO cập nhật Supplier
  // ===========================
  [AutoMapTo(typeof(Supplier))]
  public class UpdateSupplierDto : EntityDto<int>
  {
    [Required]
    public string SupplierCode { get; set; }

    [Required]
    public string SupplierName { get; set; }

    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }

    public string TaxCode { get; set; }
    public string Note { get; set; }

    public int IsActive { get; set; }
  }

  // ===========================
  // DTO request phân trang + lọc + sort  
  // ===========================
  public class PagedSupplierResultRequestDto : PagedAndSortedResultRequestDto
  {
    // Tìm kiếm theo tên hoặc mã NCC
    public string Keyword { get; set; }

    // Lọc theo trạng thái: 1 = hoạt động, 0 = ngừng
    public int? IsActive { get; set; }
  }
}
