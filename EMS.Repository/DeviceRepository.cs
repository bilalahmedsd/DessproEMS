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
    public class DeviceRepository:BaseRepository,IDeviceRepository
    {
        public DeviceRepository(EMSContext eMSContext)
        {
            DBEMSContext = eMSContext;
        }
        public async Task<List<DeviceDTO>> Get()
        {
            var res = await DBEMSContext.Devices.Where(x => x.IsDeleted == false).ToListAsync();
            return res.ToJson().FromJson<List<DeviceDTO>>();
        }

        public async Task<DeviceDTO> Get(int id)
        {
            var res = DBEMSContext.Devices.FirstOrDefaultAsync(x => x.Id == id);
            return res.ToJson().FromJson<DeviceDTO>();
        }
       

        public async Task Insert(DeviceDTO obj)
        {
            await DBEMSContext.Devices.AddAsync(obj.ToJson().FromJson<Device>());
            await DBEMSContext.SaveChangesAsync();
        }

        public async Task Update(DeviceDTO obj)
        {
            var existingUnit = await DBEMSContext.Devices.FirstOrDefaultAsync(x => x.Id == obj.Id);

            if (existingUnit == null)
                throw new Exception("Unit not found!");


            existingUnit.IsActive = obj.IsActive;
           existingUnit.UpdatedAt = obj.UpdatedAt;
            existingUnit.UpdatedBy = obj.UpdatedBy;
            existingUnit.IsDeleted = obj.IsDeleted;
            existingUnit.Name = obj.Name;
            existingUnit.ChannelName = obj.ChannelName;
            existingUnit.FkGatewayId = obj.FkGatewayId;
            existingUnit.SerialNo = obj.SerialNo;
            existingUnit.Status = obj.Status;
            existingUnit.ConsumptionUnit = obj.ConsumptionUnit;
            existingUnit.DisplaySequence = obj.DisplaySequence;
            existingUnit.SerialPort = obj.SerialPort;
            existingUnit.OfflineDuration = obj.OfflineDuration;
            existingUnit.OfflineTime = obj.OfflineTime;
            


            // ✅ Instead of Update(), use Attach() to avoid tracking issues
            DBEMSContext.Attach(existingUnit);
            DBEMSContext.Entry(existingUnit).State = EntityState.Modified;

            await DBEMSContext.SaveChangesAsync();
        }


        public async Task Delete(int id)
        {
            var existingUnit = await DBEMSContext.Devices.FirstOrDefaultAsync(x => x.Id == id);

            if (existingUnit == null)
                throw new Exception("Gateway not found!");

            existingUnit.IsDeleted = true;

            await DBEMSContext.SaveChangesAsync();
        }

    }
}
