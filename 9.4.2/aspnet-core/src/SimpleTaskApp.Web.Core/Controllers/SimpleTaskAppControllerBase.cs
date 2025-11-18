using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace SimpleTaskApp.Controllers
{
    public abstract class SimpleTaskAppControllerBase: AbpController
    {
        protected SimpleTaskAppControllerBase()
        {
            LocalizationSourceName = SimpleTaskAppConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
