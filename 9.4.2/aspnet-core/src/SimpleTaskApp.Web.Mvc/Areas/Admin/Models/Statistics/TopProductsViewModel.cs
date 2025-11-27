using SimpleTaskApp.Statistics.Dto;
using System.Collections.Generic;

namespace SimpleTaskApp.Web.Areas.Admin.Models.Statistics
{
  public class TopProductsViewModel
  {
    public List<TopProductDto> TopProducts { get; set; }
    public StatisticsFilterDto Filter { get; set; }
  }
  public class TopCustomersViewModel
  {
    public List<TopCustomerDto> TopCustomers { get; set; }
    public StatisticsFilterDto Filter { get; set; }
  }
  // ViewModel cho doanh thu theo danh mục & thương hiệu
  public class CategoryRevenueViewModel
  {
    public List<CategoryRevenueDto> Categories { get; set; } = new List<CategoryRevenueDto>();
    public StatisticsFilterDto Filter { get; set; } = new StatisticsFilterDto();
  }
}
