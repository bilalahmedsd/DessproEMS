using EMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Interfaces
{
    public interface ICompanyRepository
    {
         Task<CompanyDTO> GetCompanyByCompanyID(int CompanyId);
        
    }
}
