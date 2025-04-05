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
        Task<List<DeviceDTO>> Get(int companyId);
        Task<DeviceDTO> GetWithId(int id, int companyId);

        Task<List<DeviceDTO>> GetDevicesWithGateways(int GatewayId, int companyId);
        Task<List<DeviceDTO>> GetDevicesWithoutGatewayId(int companyId);
        Task<List<DeviceDTO>> GetDevicesWithMultipleGateways(List<int> gatewayIds, int companyId);

        Task Insert(DeviceDTO obj);
        Task Update(DeviceDTO obj);
        Task Delete(int id);
    }
}
