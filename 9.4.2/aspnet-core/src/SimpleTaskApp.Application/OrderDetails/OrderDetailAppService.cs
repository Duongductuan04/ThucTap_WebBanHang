
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
  [AbpAuthorize]
  public class OrderDetailAppService : ApplicationService, IOrderDetailAppService
  {
    private readonly IRepository<OrderDetail, int> _orderDetailRepository;

    public OrderDetailAppService(IRepository<OrderDetail, int> orderDetailRepository)
    {
      _orderDetailRepository = orderDetailRepository;
    }

    // Lấy danh sách phân trang OrderDetail
    public async Task<PagedResultDto<OrderDetailDto>> GetAllAsync(PagedOrderDetailResultRequestDto input)
    {
      var query = _orderDetailRepository.GetAllIncluding(od => od.MobilePhone);

      var totalCount = await query.CountAsync();

      var items = await query
          .OrderBy(od => od.OrderId)
          .Skip(input.SkipCount)
          .Take(input.MaxResultCount)
          .ToListAsync();

      var dtoList = items.Select(MapToOrderDetailDto).ToList();

      return new PagedResultDto<OrderDetailDto>(totalCount, dtoList);
    }

    // Hàm map OrderDetail -> OrderDetailDto
    private OrderDetailDto MapToOrderDetailDto(OrderDetail od)
    {
      if (od == null) return null;

      return new OrderDetailDto
      {
        Id = od.Id,
        OrderId = od.OrderId,
        MobilePhoneId = od.MobilePhoneId,
        MobilePhoneName = od.MobilePhone?.Name,
        ImageUrl = od.MobilePhone?.ImageUrl,
        Quantity = od.Quantity,
        UnitPrice = od.UnitPrice,
      };
    }
  }
}
