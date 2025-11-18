using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using SimpleTaskApp.Controllers;

namespace SimpleTaskApp.Areas.Admin.Controllers
{
    [Area("Admin")]

    [AbpMvcAuthorize]
    public class HomeController : SimpleTaskAppControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
