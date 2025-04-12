using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Core.Services;
using EMS.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        //public UserDTO? CurrentUser;

        public DateTime CurrentDateTime { get; set; } = DateTime.Now;
        public IUserServices userServices;
        public BaseController(IUserServices services)
        {
            userServices = services;
        }


    }
}
