using EMS.Core.Helpers;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace EMS.Repository
{
    public class UsersRepository : BaseRepository, IUsersRepository
    {
        public UsersRepository(EMSContext eMSContext)
        {
            DBEMSContext = eMSContext;
        }

        public async Task<UserDTO> Get(int id)
        {
            var user = await DBEMSContext.Users.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (user == null)
                return null;
            //user.Password = null;
            return user.ToJson().FromJson<UserDTO>();
        }

        public async Task<UserDTO> Validate(string userName, string password)
        {
            var user = await DBEMSContext.Users.Where(x => x.Email== userName && x.Password == password && x.IsActive ==true && x.IsDeleted == false).FirstOrDefaultAsync();
            if (user == null)
                return null;
            user.Password = null;
            return user.ToJson().FromJson<UserDTO>();
        }
    }
}
