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
    public class UnitRepository : BaseRepository, IUnitRepository
    {
        public UnitRepository(EMSContext eMSContext)
        {
            DBEMSContext = eMSContext;
        }
        public async Task<List<UnitDTO>> Get()
        {
            var res = await DBEMSContext.Units.Where(x => x.IsDeleted == false).ToListAsync();
            return res.ToJson().FromJson<List<UnitDTO>>();
        }

        public async Task<UnitDTO> Get(int id)
        {
            var res = await DBEMSContext.Units.FirstOrDefaultAsync(x => x.Id == id);
            return res.ToJson().FromJson<UnitDTO>();
        }

        public async Task Insert(UnitDTO obj)
        {
            await DBEMSContext.Units.AddAsync(obj.ToJson().FromJson<Unit>());
            await DBEMSContext.SaveChangesAsync();
        }

        public async Task Update(UnitDTO obj)
        {
            var res = await Get(obj.Id.Value);
            res.FkProjectManagement = obj.FkProjectManagement;
            res.IsActive = obj.IsActive;
            res.Name = obj.Name;
            res.SerialNumber = obj.SerialNumber;
            res.Status = obj.Status;
            res.UpdatedAt = obj.UpdatedAt;
            res.UpdatedBy = obj.UpdatedBy;
            res.IsDeleted = obj.IsDeleted;
            DBEMSContext.Update(res);
            await DBEMSContext.SaveChangesAsync();
        }
    }
}
