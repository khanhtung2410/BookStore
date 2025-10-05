using System.Collections.Generic;
using Bookstore.Roles.Dto;

namespace Bookstore.Web.Models.Users
{
    public class UserListViewModel
    {
        public IReadOnlyList<RoleDto> Roles { get; set; }
    }
}
