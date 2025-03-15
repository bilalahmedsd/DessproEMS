using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EMS.Core.Services
{
    public class UserService : IUserServices
    {
        IHttpContextAccessor _HttpContextAccessor;
        IUsersRepository UsersRepository;
        UserDTO userDTO;
        public UserService(IHttpContextAccessor httpContextAccessor, IUsersRepository usersRepository)
        {
            _HttpContextAccessor = httpContextAccessor;
            UsersRepository = usersRepository;
        }
        public UserDTO GetUser()
        {
            if (userDTO != null) return userDTO;
            if (_HttpContextAccessor.HttpContext.User != null)
            {
                int id = Convert.ToInt32(_HttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value);
                userDTO =  UsersRepository.Get(id).GetAwaiter().GetResult();
                return userDTO;
            }
            return null;
        }
    }
}
