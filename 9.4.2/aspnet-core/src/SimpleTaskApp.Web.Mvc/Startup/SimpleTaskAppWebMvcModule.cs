using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using SimpleTaskApp.Configuration;

namespace SimpleTaskApp.Web.Startup
{
    [DependsOn(typeof(SimpleTaskAppWebCoreModule))]
    public class SimpleTaskAppWebMvcModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public SimpleTaskAppWebMvcModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.Navigation.Providers.Add<SimpleTaskAppNavigationProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(SimpleTaskAppWebMvcModule).GetAssembly());
        }
    }
}
