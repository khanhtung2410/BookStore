using Abp.Application.Services.Dto;

namespace Bookstore.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; } = string.Empty;
    }
}

