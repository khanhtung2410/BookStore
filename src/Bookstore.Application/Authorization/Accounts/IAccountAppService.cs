using System.Threading.Tasks;
using Abp.Application.Services;
using Bookstore.Authorization.Accounts.Dto;

namespace Bookstore.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
