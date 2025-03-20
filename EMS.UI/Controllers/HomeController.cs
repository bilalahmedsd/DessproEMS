using EMS.UI.CustomFilter;
using Microsoft.AspNetCore.Mvc;

namespace EMS.UI.Controllers
{
    [AuthorizationFilter("1")]
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
