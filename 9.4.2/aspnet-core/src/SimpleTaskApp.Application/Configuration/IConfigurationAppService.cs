using System.Threading.Tasks;
using SimpleTaskApp.Configuration.Dto;

namespace SimpleTaskApp.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
