using Abp.AspNetCore.Mvc.ViewComponents;

namespace Bookstore.Web.Views
{
    public abstract class BookstoreViewComponent : AbpViewComponent
    {
        protected BookstoreViewComponent()
        {
            LocalizationSourceName = BookstoreConsts.LocalizationSourceName;
        }
    }
}
