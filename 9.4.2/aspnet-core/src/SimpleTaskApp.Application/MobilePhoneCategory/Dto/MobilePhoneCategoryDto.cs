using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMap(typeof(MobilePhoneCategory))]   // Map sang entity MobilePhoneCategory
    public class MobilePhoneCategoryDto : IEntityDto<int>
    {
        public int Id { get; set; }               // Khóa chính
        public string Name { get; set; }          // Tên danh mục
    }
}
