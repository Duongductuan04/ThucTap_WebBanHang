using Abp.AspNetCore.Mvc.ViewComponents;

namespace SimpleTaskApp.Areas.Admin.Views
{
    public abstract class SimpleTaskAppViewComponent : AbpViewComponent
    {
        protected SimpleTaskAppViewComponent()
        {
            LocalizationSourceName = SimpleTaskAppConsts.LocalizationSourceName;
        }
    }
}
