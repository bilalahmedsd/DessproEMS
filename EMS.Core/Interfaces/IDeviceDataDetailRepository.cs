using EMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EMS.Core.Interfaces
{
    public interface IDeviceDataDetailRepository
    {
        Task<List<DeviceDataDetailDTO>> GetDeviceDataDetailsAsync();
        Task<KeyValuePair<string, string>[]> GetHistoricDeviceDataDetailsAsync(DateTime startDate, DateTime endDate, string parameter);
        Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail(
       IEnumerable<int> projectId,
       IEnumerable<int> unitId,
       Dictionary<int, List<int>> meterId,
       DateTime startDate,
       DateTime endDate,
       string timeRange);
    }
}
