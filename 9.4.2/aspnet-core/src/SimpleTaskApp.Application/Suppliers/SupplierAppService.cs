using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.MobilePhones;
using SimpleTaskApp.Suppliers.Dto;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleTaskApp.Suppliers
{
  [AbpAuthorize(PermissionNames.Pages_Suppliers)]
  public class SupplierAppService : ApplicationService, ISupplierAppService
  {
    private readonly IRepository<Supplier, int> _supplierRepository;

    public SupplierAppService(IRepository<Supplier, int> supplierRepository)
    {
      _supplierRepository = supplierRepository;
    }

    private SupplierDto MapToDto(Supplier supplier)
    {
      if (supplier == null)
        return null;

      return new SupplierDto
      {
        Id = supplier.Id,
        SupplierCode = supplier.SupplierCode,
        SupplierName = supplier.SupplierName,
        Phone = supplier.Phone,
        Email = supplier.Email,
        Address = supplier.Address,
        TaxCode = supplier.TaxCode,
        Note = supplier.Note,
        IsActive = supplier.IsActive,
        CreationTime = supplier.CreationTime
      };
    }


    [AbpAuthorize(PermissionNames.Pages_Suppliers)]
    public async Task<PagedResultDto<SupplierDto>> GetAllAsync(PagedSupplierResultRequestDto input)
    {
      var query = _supplierRepository.GetAll();

      if (!string.IsNullOrWhiteSpace(input.Keyword))
      {
        query = query.Where(s => s.SupplierName.Contains(input.Keyword)
                               || s.SupplierCode.Contains(input.Keyword));
      }

      if (input.IsActive.HasValue)
      {
        query = query.Where(s => s.IsActive == input.IsActive.Value);
      }

      var totalCount = await query.CountAsync();

      var items = await query
          .OrderBy(s => s.SupplierName)
          .Skip(input.SkipCount)
          .Take(input.MaxResultCount)
          .ToListAsync();

      var dtoList = items.Select(MapToDto).ToList();

      return new PagedResultDto<SupplierDto>(totalCount, dtoList);
    }

    [AbpAuthorize(PermissionNames.Pages_Suppliers)]
    public async Task<SupplierDto> GetByIdAsync(int id)
    {
      var supplier = await _supplierRepository.GetAsync(id);
      return MapToDto(supplier);
    }


    [AbpAuthorize(PermissionNames.Pages_Suppliers_Create)]
    public async Task<SupplierDto> CreateAsync(CreateSupplierDto input)
    {
      var supplier = new Supplier
      {
        SupplierCode = input.SupplierCode,
        SupplierName = input.SupplierName,
        Phone = input.Phone,
        Email = input.Email,
        Address = input.Address,
        TaxCode = input.TaxCode,
        Note = input.Note,
        IsActive = input.IsActive
      };

      await _supplierRepository.InsertAsync(supplier);
      await CurrentUnitOfWork.SaveChangesAsync();

      return MapToDto(supplier);
    }

    [AbpAuthorize(PermissionNames.Pages_Suppliers_Edit)]
    public async Task<SupplierDto> UpdateAsync(UpdateSupplierDto input)
    {
      var supplier = await _supplierRepository.GetAsync(input.Id);

      supplier.SupplierCode = input.SupplierCode;
      supplier.SupplierName = input.SupplierName;
      supplier.Phone = input.Phone;
      supplier.Email = input.Email;
      supplier.Address = input.Address;
      supplier.TaxCode = input.TaxCode;
      supplier.Note = input.Note;
      supplier.IsActive = input.IsActive;

      await _supplierRepository.UpdateAsync(supplier);

      return MapToDto(supplier);
    }

    [AbpAuthorize(PermissionNames.Pages_Suppliers_Delete)]
    public async Task DeleteAsync(int id)
    {
      await _supplierRepository.DeleteAsync(id);
    }


    [AbpAuthorize(PermissionNames.Pages_Suppliers)]
    public async Task<string> GetNameAsync(int id)
    {
      var supplier = await _supplierRepository.GetAsync(id);
      return supplier?.SupplierName;
    }
  }
}
