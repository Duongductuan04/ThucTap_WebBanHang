using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using SimpleTaskApp.Configuration.Dto;

namespace SimpleTaskApp.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : SimpleTaskAppAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
