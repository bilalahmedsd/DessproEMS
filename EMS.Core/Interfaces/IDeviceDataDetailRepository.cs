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
        Task<List<DeviceDataDetailDTO>> GetFilterDeviceDataDetails(
        int[] projectIds, // Filter by Project IDs
        int[] unitIds, // Filter by Unit IDs
        int[] meterIds, // Filter by Meter IDs
        DateTime startDate, // Filter by start date
        DateTime endDate, // Filter by end date
        string timeRange); // Filter by time range (e.g., hourly, daily, monthly)
    }
}
