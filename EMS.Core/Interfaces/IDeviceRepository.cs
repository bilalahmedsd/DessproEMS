using EMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Interfaces
{
    public interface IDeviceRepository
    {
        Task<List<DeviceDTO>> Get();
        Task<DeviceDTO> Get(int id);   
        Task Insert(DeviceDTO obj);
        Task Update(DeviceDTO obj);
        Task Delete(int id);
    }
}
