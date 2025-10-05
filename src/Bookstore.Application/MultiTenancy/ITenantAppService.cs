using Abp.Application.Services;
using Bookstore.MultiTenancy.Dto;

namespace Bookstore.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

