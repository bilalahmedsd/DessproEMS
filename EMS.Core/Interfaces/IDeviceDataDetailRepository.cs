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
        Task<List<DeviceDataDetailDTO>> GetDeviceDataDetailsAsync(int deviceid);
        Task<List<DeviceDataDetailDTO>> Get(int id);

        Task<List<DeviceDataDetailDTO>> GetEnergyConspDatau1();


        Task<List<DeviceDataDetailDTO>> GetEnergyConspDatau2();

        Task<List<DeviceDataDetailDTO>> GetEnergyConspDatau3();


        Task<List<DeviceDataDetailDTO>> GetkW();

        Task<List<DeviceDTO>> GetDeviceStatus();

        Task<List<DeviceDataDetailDTO>> GetPowerLoadu1();

        Task<List<DeviceDataDetailDTO>> GetPowerLoadu2();

        Task<List<DeviceDataDetailDTO>> GetPowerLoadu3();

        Task<List<DeviceDataDetailDTO>> GetAlert();
        Task<KeyValuePair<string, string>[]> GetHistoricDeviceDataDetailsAsync(DateTime startDate, DateTime endDate, string parameter);
        Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail(
       IEnumerable<int> projectId,
       IEnumerable<int> unitId,
       Dictionary<int, List<int>> meterId,
       DateTime startDate,
       DateTime endDate,
       string timeRange);

        Task<List<UnitWiseAddressVariableSumDTO>> Getpowerloadtoday();
        Task<List<UnitWiseAddressVariableSumDTO>> Getpowerloadhourly();

        Task<List<HourlyAddressVariableSumDTO>> GetPowerLoadTodayHourly();

        Task<List<RealTimeDataDeviceUnitWise>> GetPowerConsumptionRealtime();


        Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail2(ProjectDataRequest request);

    }
}
