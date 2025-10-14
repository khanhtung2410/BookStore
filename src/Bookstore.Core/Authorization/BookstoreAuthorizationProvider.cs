using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace Bookstore.Authorization
{
    public class BookstoreAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

            context.CreatePermission(PermissionNames.Pages_Books, L("Books"));
            context.CreatePermission(PermissionNames.Pages_Books_Create, L("CreateBook"));
            context.CreatePermission(PermissionNames.Pages_Books_Update, L("UpdateBook"));
            context.CreatePermission(PermissionNames.Pages_Books_Delete, L("DeleteBook"));
            context.CreatePermission(PermissionNames.Pages_Books_View, L("ViewBook"));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, BookstoreConsts.LocalizationSourceName);
        }
    }
}
