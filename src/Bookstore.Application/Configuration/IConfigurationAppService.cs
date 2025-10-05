using System.Threading.Tasks;
using Bookstore.Configuration.Dto;

namespace Bookstore.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
