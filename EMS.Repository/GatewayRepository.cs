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
    public class GatewayRepository : BaseRepository, IGatewayRepository
    {
        public GatewayRepository(EMSContext eMSContext)
        {
            DBEMSContext = eMSContext;
        }
        public async Task<List<GatewayDTO>> Get()
        {
            var res = await DBEMSContext.Gateways.Where(x => x.IsDeleted == false).ToListAsync();
            return res.ToJson().FromJson<List<GatewayDTO>>();
        }

        public async Task<GatewayDTO> Get(int id)
        {
            var res = DBEMSContext.Gateways.FirstOrDefaultAsync(x => x.Id == id);
            return res.ToJson().FromJson<GatewayDTO>();
        }
        public async Task<List<GatewayDTO>> GetGatewayWithUnits()
        {
            var res = await (from gateway in DBEMSContext.Gateways
                             where gateway.IsDeleted == false
                             join unit in DBEMSContext.Units
                             on gateway.FkUnitId equals unit.Id
                             select new GatewayDTO
                             {
                                 Id = gateway.Id,
                                 Name = gateway.Name,
                                 ProtocolName = gateway.ProtocolName,
                                 SerialNo = gateway.SerialNo,
                                 InstantVariable = gateway.InstantVariable,
                                 AccumulatedVariable = gateway.AccumulatedVariable,
                                 Unit = new UnitDTO
                                 {
                                     Id = unit.Id,
                                     Name = unit.Name,
                                 }

                             }).ToListAsync();
            return res;
        }

        public async Task Insert(GatewayDTO obj)
        {
            await DBEMSContext.Gateways.AddAsync(obj.ToJson().FromJson<Gateway>());
            await DBEMSContext.SaveChangesAsync();
        }

        public async Task Update(GatewayDTO obj)
        {
            var existingUnit = await DBEMSContext.Gateways.FirstOrDefaultAsync(x => x.Id == obj.Id);

            if (existingUnit == null)
                throw new Exception("Unit not found!");


            existingUnit.IsActive = obj.IsActive;
            existingUnit.Name = obj.Name;
            existingUnit.ProtocolName = obj.ProtocolName;
            existingUnit.SerialNo = obj.SerialNo;
            existingUnit.AccumulatedVariable = obj.AccumulatedVariable;
            existingUnit.InstantVariable = obj.InstantVariable;
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
            var existingUnit = await DBEMSContext.Gateways.FirstOrDefaultAsync(x => x.Id == id);

            if (existingUnit == null)
                throw new Exception("Gateway not found!");

            existingUnit.IsDeleted = true;

            await DBEMSContext.SaveChangesAsync();
        }


    }
}
