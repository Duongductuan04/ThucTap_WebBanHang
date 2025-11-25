using SimpleTaskApp.Statistics.Dto;
using System.Collections.Generic;

namespace SimpleTaskApp.Web.Areas.Admin.Models.Statistics
{
  public class TopProductsViewModel
  {
    public List<TopProductDto> TopProducts { get; set; }
    public StatisticsFilterDto Filter { get; set; }
  }
}
