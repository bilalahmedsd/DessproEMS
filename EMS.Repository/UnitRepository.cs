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

        public async Task<UnitDTO> GetById(int id)
        {
            var res = await DBEMSContext.Units.FirstOrDefaultAsync(x => x.Id == id);
            return res.ToJson().FromJson<UnitDTO>();
        }

        public async Task Insert(UnitDTO obj)
        {
            await DBEMSContext.Units.AddAsync(obj.ToJson().FromJson<Unit>());
            await DBEMSContext.SaveChangesAsync();
        }
        public async Task Insert(ProjectManagementDTO obj)
        {
            await DBEMSContext.ProjectManagements.AddAsync(obj.ToJson().FromJson<ProjectManagement>());
            await DBEMSContext.SaveChangesAsync();
        }


        //public async Task Update(UnitDTO obj)
        //{
        //    var res = await Get(obj.Id.Value);
        //    if (res == null)
        //        throw new Exception("Unit not found!");
        //    res.FkProjectManagement = obj.FkProjectManagement;
        //    res.IsActive = obj.IsActive;
        //    res.Name = obj.Name;
        //    res.SerialNumber = obj.SerialNumber;
        //    res.Status = obj.Status;
        //    res.UpdatedAt = obj.UpdatedAt;
        //    res.UpdatedBy = obj.UpdatedBy;
        //    res.IsDeleted = obj.IsDeleted;
        //    DBEMSContext.Update(res);
        //    await DBEMSContext.SaveChangesAsync();
        //}

        public async Task Update(UnitDTO obj)
        {
            var existingUnit = await DBEMSContext.Units.FirstOrDefaultAsync(x => x.Id == obj.Id);

            if (existingUnit == null)
                throw new Exception("Unit not found!");

            existingUnit.FkProjectManagement = obj.FkProjectManagement;
            existingUnit.IsActive = obj.IsActive;
            existingUnit.Name = obj.Name;
            existingUnit.SerialNumber = obj.SerialNumber;
            existingUnit.Status = obj.Status;
            existingUnit.UpdatedAt = obj.UpdatedAt;
            existingUnit.UpdatedBy = obj.UpdatedBy;
            existingUnit.IsDeleted = obj.IsDeleted;

            // ✅ Instead of Update(), use Attach() to avoid tracking issues
            DBEMSContext.Attach(existingUnit);
            DBEMSContext.Entry(existingUnit).State = EntityState.Modified;

            await DBEMSContext.SaveChangesAsync();
        }
        public async Task Delete(int id)
        {
            var existingUnit = await DBEMSContext.Units.FirstOrDefaultAsync(x => x.Id == id);

            if (existingUnit == null)
                throw new Exception("Unit not found!");

            existingUnit.IsDeleted = true;

            await DBEMSContext.SaveChangesAsync();
        }
    }
}
