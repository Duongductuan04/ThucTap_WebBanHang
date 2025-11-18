using System.Threading.Tasks;
using SimpleTaskApp.Models.TokenAuth;
using SimpleTaskApp.Areas.Admin.Controllers;
using Shouldly;
using Xunit;

namespace SimpleTaskApp.Web.Tests.Controllers
{
    public class HomeController_Tests: SimpleTaskAppWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}