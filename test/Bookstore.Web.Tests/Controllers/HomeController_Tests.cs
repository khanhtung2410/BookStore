using System.Threading.Tasks;
using Bookstore.Models.TokenAuth;
using Bookstore.Web.Controllers;
using Shouldly;
using Xunit;

namespace Bookstore.Web.Tests.Controllers
{
    public class HomeController_Tests: BookstoreWebTestBase
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