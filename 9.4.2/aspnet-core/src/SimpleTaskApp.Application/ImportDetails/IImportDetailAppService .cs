using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SimpleTaskApp.MobilePhones.Dto;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    public interface IImportDetailAppService : IApplicationService
    {
        // Lấy danh sách chi tiết phiếu nhập có phân trang + tìm kiếm
        Task<PagedResultDto<ImportDetailDto>> GetAllAsync(PagedImportDetailResultRequestDto input);

        // Lấy chi tiết 1 bản ghi cụ thể
        Task<ImportDetailDto> GetAsync(EntityDto<int> input);

        // Thêm mới chi tiết phiếu nhập

        // Cập nhật chi tiết phiếu nhập

        // Lấy tên sản phẩm hoặc mã phiếu nhập (tùy theo mục đích)

        // Xóa chi tiết phiếu nhập
    }
}
