using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using SimpleTaskApp.EntityFrameworkCore;
using SimpleTaskApp.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace SimpleTaskApp.Web.Tests
{
    [DependsOn(
        typeof(SimpleTaskAppWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class SimpleTaskAppWebTestModule : AbpModule
    {
        public SimpleTaskAppWebTestModule(SimpleTaskAppEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(SimpleTaskAppWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(SimpleTaskAppWebMvcModule).Assembly);
        }
    }
}