using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace SimpleTaskApp.MobilePhones.Dto
{
  [AutoMap(typeof(ImportDetail))]
  public class ImportDetailDto : IEntityDto<int>
  {
    public int Id { get; set; }

    public int ImportId { get; set; }

    public int MobilePhoneId { get; set; }
    public string MobilePhoneName { get; set; } // tên sản phẩm

    public int? MobilePhoneColorId { get; set; } // id màu (nếu có)
    public string MobilePhoneColorName { get; set; } // tên màu để hiển thị
    public int? MobilePhoneColorStockQuantity { get; set; } // tồn kho của màu
    public int MobilePhoneStockQuantity { get; set; } // thêm tồn kho sản phẩm chung

    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
  }
}
