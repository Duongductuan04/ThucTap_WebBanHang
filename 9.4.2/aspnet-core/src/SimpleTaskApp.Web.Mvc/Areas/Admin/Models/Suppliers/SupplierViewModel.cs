using SimpleTaskApp.Suppliers.Dto;

namespace SimpleTaskApp.Areas.Admin.Models.Suppliers
{
  public class SupplierViewModel
  {
    // Dùng cho Create Modal
    public CreateSupplierDto CreateSupplier { get; set; }

    // Dùng cho Edit Modal
    public SupplierDto Supplier { get; set; }

    public SupplierViewModel()
    {
      CreateSupplier = new CreateSupplierDto();
    }
  }
}
