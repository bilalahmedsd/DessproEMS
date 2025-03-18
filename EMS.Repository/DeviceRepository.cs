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
        public async Task<List<DeviceDTO>> Get(int companyId)
        {
            var res = await DBEMSContext.Devices.Where(x => x.IsDeleted == false && x.FkCompanyId == companyId).ToListAsync();
            return res.ToJson().FromJson<List<DeviceDTO>>();
        }

        public async Task<DeviceDTO> GetWithId(int id,int companyId)
        {
            var res = DBEMSContext.Devices.FirstOrDefaultAsync(x => x.Id == id && x.FkCompanyId == companyId);
            return res.ToJson().FromJson<DeviceDTO>();
        }
       

        public async Task Insert(DeviceDTO obj)
        {
            await DBEMSContext.Devices.AddAsync(obj.ToJson().FromJson<Device>());
            await DBEMSContext.SaveChangesAsync();
        }
        public async Task Insert(GatewayDTO obj)

        {
            await DBEMSContext.Gateways.AddAsync(obj.ToJson().FromJson<Gateway>());
            await DBEMSContext.SaveChangesAsync();
        }
        public async Task Update(DeviceDTO obj)
        {
            var existingUnit = await DBEMSContext.Devices.FirstOrDefaultAsync(x => x.Id == obj.Id);

            if (existingUnit == null)
                throw new Exception("Device not found!");


            existingUnit.IsActive = obj.IsActive;
           existingUnit.UpdatedAt = obj.UpdatedAt;
            existingUnit.UpdatedBy = obj.UpdatedBy;
            existingUnit.IsDeleted = obj.IsDeleted;
            existingUnit.Name = obj.Name;
            existingUnit.ChannelName = obj.ChannelName;
            existingUnit.FkGatewayId = obj.FkGatewayId;
            existingUnit.FkUnitId = obj.FkUnitId;
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
        public async Task<List<DeviceDTO>> GetDevicesWithGateways(int GatewayId,int companyId)
        {
            var result = await (from device in DBEMSContext.Devices
                                
                                join gateway in DBEMSContext.Gateways
                                on device.FkGatewayId equals gateway.Id
                                where device.IsDeleted == false && gateway.IsDeleted == false
                                && device.FkGatewayId == GatewayId && gateway.FkCompanyId == companyId
                                select new DeviceDTO
                                {
                                    Id = device.Id,
                                    Name = device.Name,
                                    ChannelName = device.ChannelName,
                                    ConsumptionUnit = device.ConsumptionUnit,
                                    DisplaySequence = device.DisplaySequence,
                                    SerialPort = device.SerialPort,
                                    SerialNo = device.SerialNo,
                                    Status = device.Status,
                                    OfflineTime = device.OfflineTime,
                                    OfflineDuration = device.OfflineDuration,
                                    IsActive = device.IsActive,
                                    IsDeleted = device.IsDeleted,
                                    CreatedBy = device.CreatedBy,
                                    CreatedAt = device.CreatedAt,
                                    UpdatedBy = device.UpdatedBy,
                                    UpdatedAt = device.UpdatedAt,
                                    FkCompanyId = device.FkCompanyId,
                                    FkGatewayId = device.FkGatewayId,
                                    FkUnitId = device.FkUnitId,

                                    // ✅ Include Gateway Details
                                    Gateway = new GatewayDTO
                                    {
                                        Id = gateway.Id,
                                        Name = gateway.Name,
                                        IsActive = gateway.IsActive,
                                        IsDeleted = gateway.IsDeleted,
                                        CreatedBy = gateway.CreatedBy,
                                        CreatedAt = gateway.CreatedAt,
                                        UpdatedBy = gateway.UpdatedBy,
                                        UpdatedAt = gateway.UpdatedAt,
                                        ProtocolName = gateway.ProtocolName,
                                        SerialNo = gateway.SerialNo,
                                        AccumulatedVariable = gateway.AccumulatedVariable,
                                        InstantVariable = gateway.InstantVariable,
                                        FkCompanyId = gateway.FkCompanyId,
                                        FkUnitId = gateway.FkUnitId
                                    }
                                }).ToListAsync();

            return result;
        }


    }
}
