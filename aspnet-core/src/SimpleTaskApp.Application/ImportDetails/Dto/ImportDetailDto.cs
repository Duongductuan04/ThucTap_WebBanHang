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
        public string MobilePhoneName { get; set; }   // tên sản phẩm (map join nếu cần)
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
    }
}
