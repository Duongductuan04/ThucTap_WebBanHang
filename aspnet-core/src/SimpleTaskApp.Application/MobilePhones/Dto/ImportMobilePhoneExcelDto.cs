using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones.Dto
{
  public class ImportMobilePhoneExcelDto
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; } = 0; // mặc định 0
    public string Brand { get; set; }
    public int CategoryId { get; set; }  // luôn có giá trị
    public bool IsNew { get; set; }
    public bool IsOnSale { get; set; }
    public DateTime? SaleStart { get; set; }
    public DateTime? SaleEnd { get; set; }
    public string Colors { get; set; }   // "Đen:#000000|Trắng:#FFFFFF"
  }
}
