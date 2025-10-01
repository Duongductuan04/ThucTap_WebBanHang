using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SimpleTaskApp.MobilePhones.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    public interface IOrderAppService : IApplicationService
    {
        // Tạo order mới
        Task<OrderDto> CreateAsync(CreateOrderDto input);

        // Cập nhật thông tin order (không sửa OrderDetail)
        Task<OrderDto> UpdateAsync(UpdateOrderDto input);

        // Xóa order
        Task DeleteAsync(EntityDto<int> input);

        // Lấy order theo Id, bao gồm chi tiết
        Task<OrderDto> GetAsync(EntityDto<int> input);

        // Lấy danh sách order có phân trang
        Task<PagedResultDto<OrderDto>> GetAllAsync(PagedOrderResultRequestDto input);

    }
}
