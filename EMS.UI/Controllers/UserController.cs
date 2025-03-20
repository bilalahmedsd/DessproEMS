using Microsoft.AspNetCore.Mvc;

namespace EMS.UI.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
