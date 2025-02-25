using EMS.Core.Helpers;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Repository
{
    public class ProjectManagementRepository : BaseRepository, IProjectManagementRepository
    {
        public ProjectManagementRepository(EMSContext eMSContext)
        {
            DBEMSContext = eMSContext;
        }
        public async Task<List<ProjectManagementDTO>> Get()
        {
           var res = await DBEMSContext.ProjectManagements.Where(x => x.IsDeleted == false).ToListAsync();
            return res.ToJson().FromJson<List<ProjectManagementDTO>>();
        }

        public async Task<ProjectManagementDTO> Get(int id)
        {
            var res = DBEMSContext.ProjectManagements.FirstOrDefaultAsync(x => x.Id == id);
            return res.ToJson().FromJson<ProjectManagementDTO>();
        }

        public async Task Insert(ProjectManagementDTO obj)
        {
          await DBEMSContext.ProjectManagements.AddAsync(obj.ToJson().FromJson<ProjectManagement>());
            await DBEMSContext.SaveChangesAsync();
        }

      

        public async Task Update(ProjectManagementDTO obj)
        {
            var res = await Get(obj.Id.Value);
            res.FkCompanyId = obj.FkCompanyId;
            res.ProjectName = obj.ProjectName;
            res.CustomerName = obj.CustomerName;
            res.Address = obj.Address;
            res.Principal = obj.Principal;
            res.IsActive = obj.IsActive;
            res.UpdatedAt = obj.UpdatedAt;
            res.UpdatedBy = obj.UpdatedBy;
            res.IsDeleted = obj.IsDeleted;
            DBEMSContext.Update(res);  
            await DBEMSContext.SaveChangesAsync();
        }

       
    }
}
