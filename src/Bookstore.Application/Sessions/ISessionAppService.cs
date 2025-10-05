using System.Threading.Tasks;
using Abp.Application.Services;
using Bookstore.Sessions.Dto;

namespace Bookstore.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
