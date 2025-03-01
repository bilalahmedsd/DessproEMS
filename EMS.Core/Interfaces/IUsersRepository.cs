
using EMS.Core.Models;

namespace EMS.Core.Interfaces
{
    public interface IUsersRepository
    {
        public Task<UserDTO> Validate(string userName, string password);
        public Task<UserDTO> Get(int id);
    }
}
