using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Bookstore.Web.Views
{
    public abstract class BookstoreRazorPage<TModel> : AbpRazorPage<TModel>
    {
        [RazorInject]
        public IAbpSession AbpSession { get; set; }

        protected BookstoreRazorPage()
        {
            LocalizationSourceName = BookstoreConsts.LocalizationSourceName;
        }
    }
}
