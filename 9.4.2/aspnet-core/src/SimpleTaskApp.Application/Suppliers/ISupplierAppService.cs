using Abp.Application.Services.Dto;
using Abp.Application.Services;
using SimpleTaskApp.Suppliers.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.Suppliers
{
  public interface ISupplierAppService : IApplicationService
  {
    // 1. Lấy danh sách phân trang + filter
    Task<PagedResultDto<SupplierDto>> GetAllAsync(PagedSupplierResultRequestDto input);

    // 2. Lấy chi tiết 1 Supplier theo Id
    Task<SupplierDto> GetByIdAsync(int id);

    // 3. Tạo mới Supplier
    Task<SupplierDto> CreateAsync(CreateSupplierDto input);

    // 4. Cập nhật Supplier
    Task<SupplierDto> UpdateAsync(UpdateSupplierDto input);

    // 5. Xóa Supplier
    Task DeleteAsync(int id);
  }
}
