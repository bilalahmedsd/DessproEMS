using EMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Interfaces
{
    public interface IDeviceRawDataRepository
    {
        Task SaveData(DeviceRawDatumDTO obj);
        DeviceRawDatumDTO GetLatest();
    }
}
