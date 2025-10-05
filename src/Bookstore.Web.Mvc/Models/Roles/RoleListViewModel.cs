using System.Collections.Generic;
using Bookstore.Roles.Dto;

namespace Bookstore.Web.Models.Roles
{
    public class RoleListViewModel
    {
        public IReadOnlyList<PermissionDto> Permissions { get; set; }
    }
}
