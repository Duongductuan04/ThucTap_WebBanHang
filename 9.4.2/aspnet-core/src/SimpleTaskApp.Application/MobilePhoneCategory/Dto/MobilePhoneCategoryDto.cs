using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace SimpleTaskApp.MobilePhones.Dto
{
    [AutoMap(typeof(MobilePhoneCategory))]   // Map sang entity MobilePhoneCategory
    public class MobilePhoneCategoryDto : IEntityDto<int>
    {
        public int Id { get; set; }               // Khóa chính
        public string Name { get; set; }          // Tên danh mục
    }
  public class CreateMobilePhoneCategoryDto
  {
    [Required(ErrorMessage = "Name_Required")]
    public string Name { get; set; }
  }
  public class UpdateMobilePhoneCategoryDto : IEntityDto<int>
  {
    public int Id { get; set; }
    [Required(ErrorMessage = "Name_Required")]
    public string Name { get; set; }
  }

}
