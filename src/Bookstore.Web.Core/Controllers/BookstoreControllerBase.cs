using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Bookstore.Controllers
{
    public abstract class BookstoreControllerBase: AbpController
    {
        protected BookstoreControllerBase()
        {
            LocalizationSourceName = BookstoreConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
