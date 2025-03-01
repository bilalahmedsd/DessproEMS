using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public readonly UserDTO? CurrentUser;
        public BaseController()
        {

            if (User != null)
            {
                int id = Convert.ToInt32(User.FindFirst(ClaimTypes.Name)?.Value);
                CurrentUser = IoC.Get<IUsersRepository>().Get(id).GetAwaiter().GetResult();
            }
        }

    }
}
