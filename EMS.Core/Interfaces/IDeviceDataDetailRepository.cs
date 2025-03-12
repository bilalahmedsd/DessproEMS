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
        Task<List<DeviceDataDetailDTO>> GetfilterDeviceDataDetails(
            int? projectId, int? meterId, int? unitId, DateTime startDate, DateTime endDate, string? timeRange);

    }
}
