using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using Bookstore.Controllers;

namespace Bookstore.Web.Controllers
{
    [AbpMvcAuthorize]
    public class AboutController : BookstoreControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
	}
}
