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
        [HttpGet("Test")]
        public async Task<IActionResult> TestLogger([FromServices] ILoggerService logger)
        {
            try
            {
                throw new InvalidDataException("Hello Zohaib ");
            }
            catch (Exception ex)
            {
                await logger.LogErrorAsync(ex);
                return Ok("Error logged");
            }
        }

        [HttpGet("Get")]
        public IActionResult Get()
        {
            return Ok("Hello");
        }
    }
}
