using EMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Interfaces
{
    public interface IDeviceDataDetailRepository
    {
        Task<List<DeviceDataDetailDTO>> GetDeviceDataDetailsAsync();
        Task<List<DeviceDataDetailDTO>> GetHistoricDeviceDataDetailsAsync(DateTime startDate, DateTime endDate);
       // Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail(
       //IEnumerable<int> projectId,
       //IEnumerable<int> unitId,
       //Dictionary<int, List<int>> meterId,
       //DateTime startDate,
       //DateTime endDate,
       //string timeRange);
        Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail2(ProjectDataRequest request);

    }
}
