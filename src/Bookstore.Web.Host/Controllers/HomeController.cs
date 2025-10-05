using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abp;
using Abp.Extensions;
using Abp.Notifications;
using Abp.Timing;
using Abp.Web.Security.AntiForgery;
using Bookstore.Controllers;
using Bookstore.Books;

namespace Bookstore.Web.Host.Controllers
{
    public class HomeController : BookstoreControllerBase
    {
    }
}
