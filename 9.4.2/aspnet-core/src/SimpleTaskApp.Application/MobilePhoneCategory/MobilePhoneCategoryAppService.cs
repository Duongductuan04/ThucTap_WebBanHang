using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SimpleTaskApp.Authorization;
using SimpleTaskApp.MobilePhones.Dto;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTaskApp.MobilePhones
{
  [AbpAuthorize(PermissionNames.Pages_MobilePhoneCategory)]
  public class MobilePhoneCategoryAppService : ApplicationService, IMobilePhoneCategoryAppService
  {
    private readonly IRepository<MobilePhoneCategory, int> _categoryRepository;

    public MobilePhoneCategoryAppService(IRepository<MobilePhoneCategory, int> categoryRepository)
    {
      _categoryRepository = categoryRepository;
    }



    public async Task<PagedResultDto<MobilePhoneCategoryDto>> GetAllAsync(PagedMobilePhoneCategoryResultRequestDto input)
    {
      var query = _categoryRepository.GetAll();

      if (!string.IsNullOrWhiteSpace(input.Keyword))
      {
        query = query.Where(c => c.Name.Contains(input.Keyword));
      }

      var totalCount = await query.CountAsync();

      var items = await query
          .OrderBy(c => c.Name)
          .Skip(input.SkipCount)
          .Take(input.MaxResultCount)
          .ToListAsync();

      var dtoList = items.Select(MapToDto).ToList();

      return new PagedResultDto<MobilePhoneCategoryDto>(totalCount, dtoList);
    }


 
    public async Task<MobilePhoneCategoryDto> GetAsync(EntityDto<int> input)
    {
      var category = await _categoryRepository.GetAsync(input.Id);
      return MapToDto(category);
    }



    [AbpAuthorize(PermissionNames.Pages_MobilePhoneCategory_Create)]
    public async Task<MobilePhoneCategoryDto> CreateAsync(CreateMobilePhoneCategoryDto input)
    {
      var category = new MobilePhoneCategory
      {
        Name = input.Name
      };

      await _categoryRepository.InsertAsync(category);
      await CurrentUnitOfWork.SaveChangesAsync();

      return MapToDto(category);
    }


    [AbpAuthorize(PermissionNames.Pages_MobilePhoneCategory_Edit)]
    public async Task<MobilePhoneCategoryDto> UpdateAsync(UpdateMobilePhoneCategoryDto input)
    {
      var category = await _categoryRepository.GetAsync(input.Id);

      category.Name = input.Name;

      await _categoryRepository.UpdateAsync(category);

      return MapToDto(category);
    }


 
    [AbpAuthorize(PermissionNames.Pages_MobilePhoneCategory_Delete)]
    public async Task DeleteAsync(EntityDto<int> input)
    {
      await _categoryRepository.DeleteAsync(input.Id);
    }


    public async Task<string> GetNameAsync(int id)
    {
      var category = await _categoryRepository.GetAsync(id);
      return category?.Name;
    }


    private MobilePhoneCategoryDto MapToDto(MobilePhoneCategory category)
    {
      if (category == null)
        return null;

      return new MobilePhoneCategoryDto
      {
        Id = category.Id,
        Name = category.Name
      };
    }
  }
}
