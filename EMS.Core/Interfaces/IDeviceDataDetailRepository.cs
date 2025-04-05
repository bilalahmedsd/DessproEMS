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
        Task<List<DeviceDataDetailDTO>> Get(int id);
        Task<List<DeviceDataDetailDTO>> GetHistoricDeviceDataDetailsAsync(DateTime startDate, DateTime endDate);
<<<<<<< HEAD
        Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail(
       IEnumerable<int> projectId,
       IEnumerable<int> unitId,
       Dictionary<int, List<int>> meterId,
       DateTime startDate,
       DateTime endDate,
       string timeRange);

        Task<List<UnitWiseAddressVariableSumDTO>> Getpowerloadtoday();
        Task<List<UnitWiseAddressVariableSumDTO>> Getpowerloadhourly();
=======
       // Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail(
       //IEnumerable<int> projectId,
       //IEnumerable<int> unitId,
       //Dictionary<int, List<int>> meterId,
       //DateTime startDate,
       //DateTime endDate,
       //string timeRange);
        Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail2(ProjectDataRequest request);

>>>>>>> 71f6c9ab34eced7079137d08c1018e4ed748b0cc
    }
}
