using EMS.Core.Helpers;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Repository
{
    public class CompanyRepository : BaseRepository, ICompanyRepository
    {
        public CompanyRepository(EMSContext eMSContext)
        {
            DBEMSContext = eMSContext;
        }
        

        public async Task<CompanyDTO> GetCompanyByCompanyID(int CompanyId)
        {
            var res= await DBEMSContext.Companies.FindAsync(CompanyId);
             return res.ToJson().FromJson<CompanyDTO>();
        }
    }
}
