using EMS.Core.Models;
using EMS.Data.Models;
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
        Task<List<DeviceDataDetailDTO>> GetDeviceDataDetailsAsync(int deviceid);
        Task<EPIConspDTO> GetEPIConspAsync(int deviceId, string range);
        Task<List<DeviceDataDetailDTO>> Get(int id);

        Task<List<PowerLoadDTO>> GetEnergyConspDatau1();

        Task<List<UnitDTO>> GetUnitsDetails();

        Task<List<DeviceDTO>> Getdevices(int unitid);

        Task<List<DeviceDataDetailDTO>> GetAddresses();

        Task<bool> AddAlertCenterData(AlertCenterData model);

        Task<bool> UpdateAlertCenterData(AlertCenterData model,int id);

        Task<bool> DeleteAlertCenterData(int id);



        Task<List<AlertCenterData>> GetAllAlertCenterData();


        Task<List<PowerLoadDTO>> GetEnergyConspDatau2();

        Task<List<PowerLoadDTO>> LoadProfile();

        Task<List<PowerLoadDTO>> LoadProfilev1();

        Task<List<PowerLoadDTO>> LoadProfilev2();


        Task<List<PowerLoadDTO>> GetEnergyConspDatau3();

        Task<List<PowerLoadDTO>> PowerConsumption();

        Task<List<PowerLoadDTO>> PowerConsumptionv1();

        Task<List<PowerLoadDTO>> PowerConsumptionv2();

        //Task<List<PowerLoadDTO>> AllUnitsConsumption();


        Task<List<PowerLoadDTO>> GetkW();

        Task<List<DeviceDTO>> GetDeviceStatus();

        Task<List<PowerLoadDTO>> GetPowerLoadu1();

        Task<List<PowerLoadDTO>> GetPowerLoadu2();

        Task<List<PowerLoadDTO>> GetPowerLoadu3();

        Task<string> GetAlert();

        Task<List<AlertCenterDTO>> GetAlertsNotices();
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
