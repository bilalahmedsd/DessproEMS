using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Api.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        [HttpGet("Get")]
        public IActionResult Get()
        {
            return Ok("Hello");
        }
    }
}
