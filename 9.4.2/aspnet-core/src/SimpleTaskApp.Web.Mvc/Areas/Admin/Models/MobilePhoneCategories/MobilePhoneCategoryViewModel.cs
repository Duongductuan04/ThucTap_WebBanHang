using SimpleTaskApp.MobilePhones.Dto;

namespace SimpleTaskApp.Areas.Admin.Models.MobilePhoneCategories
{
  public class MobilePhoneCategoryViewModel
  {
    public CreateMobilePhoneCategoryDto CreateCategory { get; set; }
    public MobilePhoneCategoryDto Category { get; set; }

    public MobilePhoneCategoryViewModel()
    {
      CreateCategory = new CreateMobilePhoneCategoryDto();
    }
  }
}
