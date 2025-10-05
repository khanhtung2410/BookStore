using Abp.Authorization;
using Bookstore.Authorization.Roles;
using Bookstore.Authorization.Users;

namespace Bookstore.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
