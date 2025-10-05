using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using Bookstore.Controllers;

namespace Bookstore.Web.Controllers
{
    [AbpMvcAuthorize]
    public class HomeController : BookstoreControllerBase
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Books");
        }
    }
}
