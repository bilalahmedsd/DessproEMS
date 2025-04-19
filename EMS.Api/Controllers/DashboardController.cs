using EMS.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Api.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        public DashboardController(IUserServices services) : base(services)
        {

        }
        [HttpGet("Get")]
        public IActionResult Get()
        {
            return Ok("Hello");
        }
    }
}
