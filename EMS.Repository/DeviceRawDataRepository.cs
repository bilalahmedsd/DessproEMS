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
    public class DeviceRawDataRepository : BaseRepository, IDeviceRawDataRepository
    {
        public DeviceRawDataRepository(EMSContext eMSContext)
        {
            DBEMSContext = eMSContext;
        }

        public DeviceRawDatumDTO GetLatest()
        {
            var res = DBEMSContext.DeviceRawData.ToList().OrderByDescending(x => x.Id).FirstOrDefault();
            return res.ToJson().FromJson<DeviceRawDatumDTO>();
        }

        public async Task SaveData(DeviceRawDatumDTO obj)
        {
            await DBEMSContext.DeviceRawData.AddAsync(obj.ToJson().FromJson<DeviceRawDatum>());
            await DBEMSContext.SaveChangesAsync();
        }
    }
}
