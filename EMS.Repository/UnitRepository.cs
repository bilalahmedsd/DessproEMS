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
        public async Task<List<UnitDTO>> Get(int companyId)
        {
            var res = await DBEMSContext.Units.Where(x => x.IsDeleted == false).ToListAsync();
            return res.ToJson().FromJson<List<UnitDTO>>();
        }

        public async Task<UnitDTO> GetById(int id, int companyID)
        {
            var res = await DBEMSContext.Units.FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
            return res.ToJson().FromJson<UnitDTO>();
        }

 public async Task Insert(UnitDTO obj)
{
    // Check if serial number already exists (excluding deleted records)
    var exists = await DBEMSContext.Units
                    .AnyAsync(x => x.SerialNumber == obj.SerialNumber && x.IsDeleted == false);

    if (exists)
        throw new Exception("This Serial Number already exists.");

    obj.IsActive = true;
    obj.IsDeleted = false;

    await DBEMSContext.Units.AddAsync(obj.ToJson().FromJson<Unit>());
    await DBEMSContext.SaveChangesAsync();
}

        public async Task Insert(ProjectManagementDTO obj)
        {
            await DBEMSContext.ProjectManagements.AddAsync(obj.ToJson().FromJson<ProjectManagement>());
            await DBEMSContext.SaveChangesAsync();
        }

        public async Task Update(UnitDTO obj)
        {
            var existingUnit = await DBEMSContext.Units.FirstOrDefaultAsync(x => x.Id == obj.Id && x.IsDeleted ==false);

            if (existingUnit == null)
                throw new Exception("Unit not found!");

            existingUnit.FkProjectManagement = obj.FkProjectManagement;
            existingUnit.IsActive = obj.IsActive;
            existingUnit.Name = obj.Name;
            existingUnit.SerialNumber = obj.SerialNumber;
            existingUnit.Status = obj.Status;
            existingUnit.UpdatedAt = obj.UpdatedAt;
            existingUnit.UpdatedBy = obj.UpdatedBy;

            DBEMSContext.Attach(existingUnit);
            DBEMSContext.Entry(existingUnit).State = EntityState.Modified;

            await DBEMSContext.SaveChangesAsync();
        }
        public async Task Delete(int id)
        {
            var existingUnit = await DBEMSContext.Units.FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);

            if (existingUnit == null)
                throw new Exception("Unit not found!");

            existingUnit.IsDeleted = true;

            await DBEMSContext.SaveChangesAsync();
        }
        public async Task<List<UnitDTO>> GetUnitsByProjectId(int projectId, int companyId)
        {

            var result = await (from unit in DBEMSContext.Units       
                                join project in DBEMSContext.ProjectManagements
                                on unit.FkProjectManagement equals project.Id
                                where unit.IsDeleted == false && project.IsDeleted == false
                                && unit.FkCompanyId == companyId && unit.FkProjectManagement == projectId
                                select new UnitDTO
                                {
                                    Id = unit.Id,
                                    Name = unit.Name,
                                    SerialNumber = unit.SerialNumber,
                                    Status = unit.Status,
                                    FkProjectManagement = unit.FkProjectManagement,
                                    IsActive = unit.IsActive,
                                    CreatedAt = unit.CreatedAt,
                                    UpdatedAt = unit.UpdatedAt,

                                    ProjectManagement = new ProjectManagementDTO
                                    {
                                        Id = project.Id,
                                        FkCompanyId = project.FkCompanyId,
                                        ProjectName = project.ProjectName,
                                        CustomerName = project.CustomerName,
                                        Address = project.Address,
                                        Principal = project.Principal,
                                        IsDeleted = project.IsDeleted,
                                        CreatedBy = project.CreatedBy,
                                        CreatedAt = project.CreatedAt,
                                        UpdatedBy = project.UpdatedBy,
                                        UpdatedAt = project.UpdatedAt,
                                        IsActive = project.IsActive
                                    }
                                }).ToListAsync();

            return result;
        }
        public async Task<List<UnitDTO>> GetUnitsWithoutProjectId( int companyId)
        {

            var result = await (from unit in DBEMSContext.Units
                                join project in DBEMSContext.ProjectManagements
                                on unit.FkProjectManagement equals project.Id
                                where unit.IsDeleted == false && project.IsDeleted == false
                                && unit.FkCompanyId == companyId
                                select new UnitDTO
                                {
                                    Id = unit.Id,
                                    Name = unit.Name,
                                    SerialNumber = unit.SerialNumber,
                                    Status = unit.Status,
                                    FkProjectManagement = unit.FkProjectManagement,
                                    IsActive = unit.IsActive,
                                    CreatedAt = unit.CreatedAt,
                                    UpdatedAt = unit.UpdatedAt,

                                    ProjectManagement = new ProjectManagementDTO
                                    {
                                        Id = project.Id,
                                        FkCompanyId = project.FkCompanyId,
                                        ProjectName = project.ProjectName,
                                        CustomerName = project.CustomerName,
                                        Address = project.Address,
                                        Principal = project.Principal,
                                        IsDeleted = project.IsDeleted,
                                        CreatedBy = project.CreatedBy,
                                        CreatedAt = project.CreatedAt,
                                        UpdatedBy = project.UpdatedBy,
                                        UpdatedAt = project.UpdatedAt,
                                        IsActive = project.IsActive
                                    }
                                }).ToListAsync();

            return result;
        }
    }
}
