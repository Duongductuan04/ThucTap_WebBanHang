using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SimpleTaskApp.MobilePhones.Dto;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
    public interface IImportAppService : IApplicationService
    {
        // ======= Tạo phiếu nhập mới (kèm danh sách chi tiết) =======
        Task<ImportDto> CreateAsync(CreateImportDto input);

        // ======= Cập nhật phiếu nhập (kèm chi tiết) =======
        Task<ImportDto> UpdateAsync(UpdateImportDto input);

        // ======= Xóa phiếu nhập =======
        Task DeleteAsync(EntityDto<int> input);

        // ======= Lấy 1 phiếu nhập theo ID =======
        Task<ImportDto> GetAsync(EntityDto<int> input);

        // ======= Phân trang & tìm kiếm phiếu nhập =======
        Task<PagedResultDto<ImportDto>> GetAllAsync(PagedImportResultRequestDto input);
    }
}
