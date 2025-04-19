using EMS.Core.Helpers;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EMS.Repository.DeviceDataDetailRepository;

namespace EMS.Repository
{
    public class DeviceDataDetailRepository : BaseRepository, IDeviceDataDetailRepository
    {
        public DeviceDataDetailRepository(EMSContext eMSContext)
        {
            DBEMSContext = eMSContext;
        }

            public async Task<List<DeviceDataDetailDTO>> Get(int id)
            {
            var endTime = DateTime.Now;
            var startTime = endTime.AddHours(-5);

            // Generate 15-minute slots between startTime and endTime
            TimeSpan slotDuration = TimeSpan.FromHours(1);
            var timeSlots = new List<DateTime>();
            var current = startTime;

            while (current < endTime)
            {
                timeSlots.Add(current);
                current = current.Add(slotDuration);
            }

            // Get the data from the last 2 hours
            var data = await DBEMSContext.DeviceDataDetails
                .Where(detail => detail.Address == "EPI"
                    && detail.CreatedAt >= startTime
                    && detail.CreatedAt < endTime
                    && detail.DeviceDataMaster.Device.FkUnitId == id)
                .Select(detail => new
                {
                    detail.CreatedAt,
                    detail.AddressVariable,
                    UnitId = id
                })
                .ToListAsync();

            // Multiply AddressVariable by 0.06 and group data into 15-minute slots
            var adjustedData = data.Select((d, index) => new
            {
                d.CreatedAt,
                AdjustedAddressVariable = d.AddressVariable * 0.06
            })
 .OrderBy(d => d.CreatedAt)
 .Where((item, index) => index % 2 == 0) // This filters to get odd indexed items (0, 2, 4, 6, etc.)
 .ToList();


            // Calculate the difference in AdjustedAddressVariable between consecutive slots
            var groupedData = timeSlots.Select(slot =>
            {
                var slotEnd = slot.Add(slotDuration);

                // Get the data for this 15-minute slot
                var dataInSlot = adjustedData.Where(d => d.CreatedAt >= slot && d.CreatedAt < slotEnd).ToList();

                if (dataInSlot.Count() > 1)
                {
                    var firstValue = dataInSlot.First().AdjustedAddressVariable;
                    var lastValue = dataInSlot.Last().AdjustedAddressVariable;

                    // Calculate the difference between the first and last adjusted value in the 15-min slot
                    var difference = lastValue - firstValue;

                    return new DeviceDataDetailDTO
                    {
                        CreatedAt = slot,
                        Address = "EPI",
                        AddressVariable = difference, // Difference between first and last adjusted value
                        UnitId = id
                    };
                }
                else
                {
                    return null; // No data in this slot
                }
            }).Where(d => d != null).ToList();

            return groupedData;


        }

        public async Task<List<DeviceDataDetailDTO>> GetEnergyConspDatau1()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddHours(-12);

            // Generate 15-minute slots between startTime and endTime
            TimeSpan slotDuration = TimeSpan.FromHours(1);
            var timeSlots = new List<DateTime>();
            var current = startTime;

            while (current < endTime)
            {
                timeSlots.Add(current);
                current = current.Add(slotDuration);
            }

            var result = new List<DeviceDataDetailDTO>();


            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).FirstOrDefaultAsync();


            if (unit != null)
            {

                var deviceIds = await DBEMSContext.Devices
                .Where(dev => dev.FkUnitId == unit.Id)
                .Select(dev => dev.Id)
                .ToListAsync();

                if (deviceIds.Any())
                {

                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
                      .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                      .Select(dm => dm.Id)
                      .ToListAsync();

                    if (dataMasterIds.Any())
                    {

                    // Get the data from the last 2 hours
                    var data = await DBEMSContext.DeviceDataDetails
                .Where(detail =>
                       detail.FkDeviceDataMasterId.HasValue
                    && dataMasterIds.Contains(detail.FkDeviceDataMasterId.Value)
                    && detail.Address == "EPI"
                    && detail.CreatedAt >= startTime
                    && detail.CreatedAt < endTime
                    && detail.DeviceDataMaster.Device.FkUnitId == unit.Id)
                .Select(detail => new
                {
                    detail.CreatedAt,
                    detail.AddressVariable,
                    UnitId = unit.Id
                })
                .ToListAsync();

            // Multiply AddressVariable by 0.06 and group data into 15-minute slots
            var adjustedData = data.Select((d, index) => new
            {
                d.CreatedAt,
                AdjustedAddressVariable = d.AddressVariable * 0.06
            })
 .OrderBy(d => d.CreatedAt)
 .Where((item, index) => index % 2 == 0) // This filters to get odd indexed items (0, 2, 4, 6, etc.)
 .ToList();


            // Calculate the difference in AdjustedAddressVariable between consecutive slots
            var groupedData = timeSlots.Select(slot =>
            {
                var slotEnd = slot.Add(slotDuration);

                // Get the data for this 15-minute slot
                var dataInSlot = adjustedData.Where(d => d.CreatedAt >= slot && d.CreatedAt < slotEnd).ToList();

                if (dataInSlot.Count() > 1)
                {
                    var firstValue = dataInSlot.First().AdjustedAddressVariable;
                    var lastValue = dataInSlot.Last().AdjustedAddressVariable;

                    // Calculate the difference between the first and last adjusted value in the 15-min slot
                    var difference = lastValue - firstValue;

                    return new DeviceDataDetailDTO
                    {
                        CreatedAt = slot,
                        Address = "EPI",
                        AddressVariable = difference, // Difference between first and last adjusted value
                        UnitId = unit.Id
                    };
                }
                else
                {
                    return null; // No data in this slot
                }
            }).Where(d => d != null).ToList();

                        result.AddRange(groupedData);

            }
                }

                    }

            return result;


        }

        public async Task<List<DeviceDataDetailDTO>> GetEnergyConspDatau2()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddHours(-12);

            // Generate 15-minute slots between startTime and endTime
            TimeSpan slotDuration = TimeSpan.FromMinutes(30);
            var timeSlots = new List<DateTime>();
            var current = startTime;

            while (current < endTime)
            {
                timeSlots.Add(current);
                current = current.Add(slotDuration);
            }

            var result = new List<DeviceDataDetailDTO>();


            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(1).FirstOrDefaultAsync();

            if (unit != null)
            {

                var deviceIds = await DBEMSContext.Devices
                .Where(dev => dev.FkUnitId == unit.Id)
                .Select(dev => dev.Id)
                .ToListAsync();

                if (deviceIds.Any())
                {

                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
                      .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                      .Select(dm => dm.Id)
                      .ToListAsync();

                    if (dataMasterIds.Any())
                    {

                        // Get the data from the last 2 hours
                        var data = await DBEMSContext.DeviceDataDetails
                    .Where(detail =>
                           detail.FkDeviceDataMasterId.HasValue
                        && dataMasterIds.Contains(detail.FkDeviceDataMasterId.Value)
                        && detail.Address == "EPI"
                        && detail.CreatedAt >= startTime
                        && detail.CreatedAt < endTime
                        && detail.DeviceDataMaster.Device.FkUnitId == unit.Id)
                    .Select(detail => new
                    {
                        detail.CreatedAt,
                        detail.AddressVariable,
                        UnitId = unit.Id
                    })
                    .ToListAsync();

                        // Multiply AddressVariable by 0.06 and group data into 15-minute slots
                        var adjustedData = data.Select((d, index) => new
                        {
                            d.CreatedAt,
                            AdjustedAddressVariable = d.AddressVariable * 0.06
                        })
             .OrderBy(d => d.CreatedAt)
             .Where((item, index) => index % 2 == 0) // This filters to get odd indexed items (0, 2, 4, 6, etc.)
             .ToList();


                        // Calculate the difference in AdjustedAddressVariable between consecutive slots
                        var groupedData = timeSlots.Select(slot =>
                        {
                            var slotEnd = slot.Add(slotDuration);

                            // Get the data for this 15-minute slot
                            var dataInSlot = adjustedData.Where(d => d.CreatedAt >= slot && d.CreatedAt < slotEnd).ToList();

                            if (dataInSlot.Count() > 1)
                            {
                                var firstValue = dataInSlot.First().AdjustedAddressVariable;
                                var lastValue = dataInSlot.Last().AdjustedAddressVariable;

                                // Calculate the difference between the first and last adjusted value in the 15-min slot
                                var difference = lastValue - firstValue;

                                return new DeviceDataDetailDTO
                                {
                                    CreatedAt = slot,
                                    Address = "EPI",
                                    AddressVariable = difference, // Difference between first and last adjusted value
                                    UnitId = unit.Id
                                };
                            }
                            else
                            {
                                return null; // No data in this slot
                            }
                        }).Where(d => d != null).ToList();

                        result.AddRange(groupedData);

                    }
                }

            }

            return result;
           

        }


        public async Task<List<DeviceDataDetailDTO>> GetEnergyConspDatau3()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddHours(-5);

            // Generate 15-minute slots between startTime and endTime
            TimeSpan slotDuration = TimeSpan.FromHours(1);
            var timeSlots = new List<DateTime>();
            var current = startTime;

            while (current < endTime)
            {
                timeSlots.Add(current);
                current = current.Add(slotDuration);
            }

            var result = new List<DeviceDataDetailDTO>();


            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(2).FirstOrDefaultAsync();

            if (unit != null)
            {

                var deviceIds = await DBEMSContext.Devices
                .Where(dev => dev.FkUnitId == unit.Id)
                .Select(dev => dev.Id)
                .ToListAsync();

                if (deviceIds.Any())
                {

                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
                      .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                      .Select(dm => dm.Id)
                      .ToListAsync();

                    if (dataMasterIds.Any())
                    {

                        // Get the data from the last 2 hours
                        var data = await DBEMSContext.DeviceDataDetails
                    .Where(detail =>
                           detail.FkDeviceDataMasterId.HasValue
                        && dataMasterIds.Contains(detail.FkDeviceDataMasterId.Value)
                        && detail.Address == "EPI"
                        && detail.CreatedAt >= startTime
                        && detail.CreatedAt < endTime
                        && detail.DeviceDataMaster.Device.FkUnitId == unit.Id)
                    .Select(detail => new
                    {
                        detail.CreatedAt,
                        detail.AddressVariable,
                        UnitId = unit.Id
                    })
                    .ToListAsync();

                        // Multiply AddressVariable by 0.06 and group data into 15-minute slots
                        var adjustedData = data.Select((d, index) => new
                        {
                            d.CreatedAt,
                            AdjustedAddressVariable = d.AddressVariable * 0.06
                        })
             .OrderBy(d => d.CreatedAt)
             .Where((item, index) => index % 2 == 0) // This filters to get odd indexed items (0, 2, 4, 6, etc.)
             .ToList();


                        // Calculate the difference in AdjustedAddressVariable between consecutive slots
                        var groupedData = timeSlots.Select(slot =>
                        {
                            var slotEnd = slot.Add(slotDuration);

                            // Get the data for this 15-minute slot
                            var dataInSlot = adjustedData.Where(d => d.CreatedAt >= slot && d.CreatedAt < slotEnd).ToList();

                            if (dataInSlot.Count() > 1)
                            {
                                var firstValue = dataInSlot.First().AdjustedAddressVariable;
                                var lastValue = dataInSlot.Last().AdjustedAddressVariable;

                                // Calculate the difference between the first and last adjusted value in the 15-min slot
                                var difference = lastValue - firstValue;

                                return new DeviceDataDetailDTO
                                {
                                    CreatedAt = slot,
                                    Address = "EPI",
                                    AddressVariable = difference, // Difference between first and last adjusted value
                                    UnitId = unit.Id
                                };
                            }
                            else
                            {
                                return null; // No data in this slot
                            }
                        }).Where(d => d != null).ToList();

                        result.AddRange(groupedData);

                    }
                }

            }

            return result;


        }

        public async Task<List<DeviceDataDetailDTO>> GetPowerLoadu1()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-11);

            var result = new List<DeviceDataDetailDTO>();

            // ✅ Get first unit only
            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id)
                    .ToListAsync();

                if (deviceIds.Any())
                {
                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
                        .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                        .Select(dm => dm.Id)
                        .ToListAsync();

                    if (dataMasterIds.Any())
                    {
                        var data = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "P" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime)
                            .Select(d => new DeviceDataDetailDTO
                            {
                                UnitId = unit.Id,
                                UnitName = unit.Name,
                                AddressVariable = d.AddressVariable / 10,
                                CreatedAt = d.CreatedAt
                            })
                            .ToListAsync();

                        result.AddRange(data);
                    }
                }
            }

            return result;


        }

        public async void getdata()
        {
            var units = await DBEMSContext.Units.ToListAsync();

            foreach(var unit in units)
            {
                if(unit != null)
                {

                }
            }
        }

        public async Task<List<DeviceDataDetailDTO>> GetPowerLoadu3()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-11);

            var result = new List<DeviceDataDetailDTO>();

            // ✅ Get first unit only
            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(2).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id)
                    .ToListAsync();

                if (deviceIds.Any())
                {
                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
                        .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                        .Select(dm => dm.Id)
                        .ToListAsync();

                    if (dataMasterIds.Any())
                    {
                        var data = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "P" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime)
                            .Select(d => new DeviceDataDetailDTO
                            {
                                UnitId = unit.Id,
                                UnitName = unit.Name,
                                AddressVariable = d.AddressVariable / 10,
                                CreatedAt = d.CreatedAt
                            })
                            .ToListAsync();

                        result.AddRange(data);
                    }
                }
            }

            return result;


        }



        public async Task<List<DeviceDataDetailDTO>> GetPowerLoadu2()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-11);

            var result = new List<DeviceDataDetailDTO>();

            // ✅ Get first unit only
            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(1).FirstOrDefaultAsync();


            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id)
                    .ToListAsync();

                if (deviceIds.Any())
                {
                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
                        .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                        .Select(dm => dm.Id)
                        .ToListAsync();

                    if (dataMasterIds.Any())
                    {
                        var data = await DBEMSContext.DeviceDataDetails
       .Where(d =>
           d.FkDeviceDataMasterId.HasValue &&
           dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
           d.Address == "P" &&
           d.CreatedAt >= startTime &&
           d.CreatedAt <= endTime)
       .OrderBy(d => d.CreatedAt) // 👈 THIS sorts the data by time
       .Select(d => new DeviceDataDetailDTO
       {
           UnitId = unit.Id,
           UnitName = unit.Name,
           AddressVariable = d.AddressVariable / 10,
           CreatedAt = d.CreatedAt
       })
       .ToListAsync();


                        result.AddRange(data);
                    }
                }
            }

            return result;


        }

        public async Task<List<DeviceDataDetailDTO>> GetAlert()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-1);

            var targetAddresses = new[] { "Ua", "Ub", "Uc" };
            var targetUnitIds = new[] { 246, 247, 248 };

            var rawData = await DBEMSContext.DeviceDataDetails
                .Where(d =>
                    targetAddresses.Contains(d.Address) &&
                    (d.DeviceDataMaster.Device.FkUnitId == 246) &&
                    d.CreatedAt >= startTime &&
                    d.CreatedAt <= endTime)
                .Select(d => new
                {
                    d.CreatedAt,
                    d.Address,
                    Value = d.AddressVariable / 10,
                    FkUnitId = d.DeviceDataMaster.Device.FkUnitId, // ✔️ Use this instead of UnitId
                    //UnitName = d.DeviceDataMaster.Device.Unit.Name,
                    DeviceName = d.DeviceDataMaster.Device.Name
                })
                .ToListAsync();

            var filtered = rawData
                .GroupBy(d => new { d.FkUnitId, d.Address }) // ✔️ Updated key
                .Where(group =>
                    group.Any(d => d.Value < 220 || d.Value > 240))
                .SelectMany(group => group)
                .OrderBy(d => d.CreatedAt)
                .Select(d => new DeviceDataDetailDTO
                {
                    CreatedAt = d.CreatedAt,
                    Address = d.Address,
                    AddressVariable = d.Value,
                    UnitId = d.FkUnitId, // ✔️ Mapping it properly now
                    //UnitName = d.UnitName,
                    DeviceName = d.DeviceName
                })
                .ToList();

            return filtered;
        }


        public async Task<List<DeviceDTO>> GetDeviceStatus()
        {
      
            var devices = await DBEMSContext.Devices.Where(x => x.IsDeleted == false).ToListAsync();

            return devices.ToJson().FromJson<List<DeviceDTO>>();
        }


        public async Task<List<DeviceDataDetailDTO>> GetkW()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddSeconds(-5);
            var units = await DBEMSContext.Units.ToListAsync();

            var result = new List<DeviceDataDetailDTO>();

            foreach (var unit in units)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id)
                    .ToListAsync();

                if (!deviceIds.Any())
                {
                    result.Add(new DeviceDataDetailDTO { UnitId = unit.Id, UnitName = unit.Name, AddressVariable = 0 });
                    continue;
                }

                var dataMasterIds = await DBEMSContext.DeviceDataMasters
                    .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                    .Select(dm => dm.Id)
                    .ToListAsync();

                if (!dataMasterIds.Any())
                {
                    result.Add(new DeviceDataDetailDTO { UnitId = unit.Id, UnitName = unit.Name, AddressVariable = 0 });
                    continue;
                }

                var data = await DBEMSContext.DeviceDataDetails
                    .Where(d =>
                        d.FkDeviceDataMasterId.HasValue &&
                        dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                        d.Address == "P" &&
                        d.CreatedAt >= startTime &&
                        d.CreatedAt <= endTime)
                    .Select(d => new DeviceDataDetailDTO
                    {
                        UnitId = unit.Id,
                        UnitName = unit.Name,
                        AddressVariable = d.AddressVariable / 10,
                        CreatedAt = d.CreatedAt
                    })
                    .ToListAsync();

                if (!data.Any())
                {
                    result.Add(new DeviceDataDetailDTO { UnitId = unit.Id, UnitName = unit.Name, AddressVariable = 0 });
                }
                else
                {
                    var latestValue = data
                        .OrderByDescending(d => d.CreatedAt) // Assuming you want the latest by time
                        .FirstOrDefault();

                    if (latestValue != null)
                    {
                        result.Add(new DeviceDataDetailDTO
                        {
                            UnitId = unit.Id,
                            UnitName = unit.Name,
                            AddressVariable = latestValue.AddressVariable
                        });
                    }
                }


            }

            return result;
        }







        public async Task<List<DeviceDataDetailDTO>> GetDeviceDataDetailsAsync(int deviceid)
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-11);

            var result = new List<DeviceDataDetailDTO>();

            // ✅ Get units from DB
            var units = await DBEMSContext.Units.ToListAsync();

            foreach (var unit in units)
            {
                if (unit != null)
                {
                    var deviceIds = await DBEMSContext.Devices
                        .Where(dev => dev.FkUnitId == unit.Id)
                        .Select(dev => dev.Id)
                        .ToListAsync();

                    if (deviceIds.Any())
                    {
                        var dataMasterIds = await DBEMSContext.DeviceDataMasters
                            .Where(dm => deviceIds.Contains(deviceid))
                            .Select(dm => dm.Id)
                            .ToListAsync();

                        if (dataMasterIds.Any())
                        {
                            // Fetch data from DB into memory
                            var data = await DBEMSContext.DeviceDataDetails
                                .Where(d =>
                                    d.FkDeviceDataMasterId.HasValue &&
                                    dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                    d.CreatedAt >= startTime &&
                                    d.CreatedAt <= endTime)
                                .Select(d => new DeviceDataDetailDTO
                                {
                                    UnitId = unit.Id,
                                    Address = d.Address,
                                    AddressVariable = d.AddressVariable,
                                    CreatedAt = d.CreatedAt,
                                    DeviceId = deviceid
                                })
                                .ToListAsync();

                            result.AddRange(data);
                        }
                    }
                }
            }

            // ✅ Group by 30-second intervals and AddressVariable
            var groupedData = result
                .Where(d => d.CreatedAt.HasValue)
                .GroupBy(d => new
                {
                    TimeBucket = new DateTime(
                        d.CreatedAt.Value.Year,
                        d.CreatedAt.Value.Month,
                        d.CreatedAt.Value.Day,
                        d.CreatedAt.Value.Hour,
                        d.CreatedAt.Value.Minute,
                        (d.CreatedAt.Value.Second / 30) * 30
                    ),
                    d.Address
                })
                .OrderBy(g => g.Key.TimeBucket)
                .Select(g =>
                {
                    var orderedGroup = g.OrderBy(x => x.CreatedAt).ToList();
                    var first = orderedGroup.First();
                    var last = orderedGroup.Last();

                    double calculatedValue = 0;
                    double lastValue = last.AddressVariable ?? 0; // Default to 0 if null
                    double firstValue = first.AddressVariable ?? 0; // Default to 0 if null

                    string addr = g.Key.Address?.ToLower() ?? "";

                    switch (addr)
                    {
                        case "ua":
                        case "ub":
                        case "uc":
                        case "p":
                            // Ensure no divide by zero occurs
                            if (lastValue != 0)
                            {
                                calculatedValue = lastValue / 10; // Divide by 20 as per original logic
                            }
                            else
                            {
                                calculatedValue = 0; // Handle case where lastValue is 0
                            }
                            break;

                        case "epi":
                            if (lastValue != 0 && firstValue != 0)
                            {
                                calculatedValue = (lastValue - firstValue) * 0.06; // EPI formula
                            }
                            break;

                        default:
                            calculatedValue = lastValue; // Default to last value
                            break;
                    }

                    return new DeviceDataDetailDTO
                    {
                        UnitId = last.UnitId,
                        Address = g.Key.Address,
                        AddressVariable = calculatedValue,
                        CreatedAt = g.Key.TimeBucket,
                        DeviceId = last.DeviceId
                    };
                })
                .ToList();

            return groupedData;
        }

        public async Task<KeyValuePair<string, string>[]> GetHistoricDeviceDataDetailsAsync(DateTime startDate, DateTime endDate, string parameter)
        {
            // Log parameters before querying
            // Convert to UTC (if your database stores timestamps in UTC)
            DateTime utcStartDate = startDate.ToUniversalTime();
            DateTime utcEndDate = endDate.ToUniversalTime();

            // Log converted dates
            Console.WriteLine($"Querying DB - Start Date (UTC): {utcStartDate}, End Date (UTC): {utcEndDate}");

            // Fetch data asynchronously and convert to array of KeyValuePair
            var data = await DBEMSContext.DeviceDataDetails
                .Where(d => d.CreatedAt >= utcStartDate && d.CreatedAt <= utcEndDate && d.Address == parameter)
                .Select(x => new KeyValuePair<string, string>(
                    x.CreatedAt.HasValue ? x.CreatedAt.Value.ToString("o") : string.Empty, // Handle null CreatedAt
                    x.AddressVariable.Value.ToString() ?? string.Empty // Handle null AddressVariable
                ))
                .ToArrayAsync();

            // Return data
            return data;
        }



        //    public async Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail(
        //IEnumerable<int> projectId,
        //IEnumerable<int> unitId,
        //Dictionary<int, List<int>> meterId,
        //DateTime startDate,
        //DateTime endDate,
        //string timeRange)
        //    {
        //        // ✅ Ensure proper date range
        //        startDate = startDate.Date;
        //        endDate = endDate.Date.AddDays(1).AddTicks(-1);

        //        var validAddresses = new HashSet<string> { "EPI", "EPE", "EQL", "EQC" };

        //        var query = (
        //            from detail in DBEMSContext.DeviceDataDetails
        //            join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
        //            join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id
        //            join unit in DBEMSContext.Units on device.FkUnitId equals unit.Id
        //            join project in DBEMSContext.ProjectManagements on unit.FkProjectManagement equals project.Id
        //            where projectId.Contains(project.Id) &&
        //                  unitId.Contains(unit.Id) &&
        //                  detail.CreatedAt >= startDate &&
        //                  detail.CreatedAt <= endDate &&
        //                  validAddresses.Contains(detail.Address)
        //            select new DeviceDataDetailDTO
        //            {
        //                Id = detail.Id,
        //                FkDeviceDataMasterId = detail.FkDeviceDataMasterId,
        //                Address = detail.Address,
        //                AddressVariable = detail.AddressVariable,
        //                CreatedAt = detail.CreatedAt,
        //                DeviceDataMaster = new DeviceDataMasterDTO
        //                {
        //                    Id = master.Id,
        //                    DeviceId = master.DeviceId,
        //                    CreatedAt = master.CreatedAt,
        //                    FkDeviceId = master.FkDeviceId,
        //                    Device = new DeviceDTO
        //                    {
        //                        Id = device.Id,
        //                        Name = device.Name,
        //                        SerialNo = device.SerialNo,
        //                        Status = device.Status,
        //                        CreatedAt = device.CreatedAt,
        //                        FkUnitId = device.FkUnitId,
        //                        Unit = new UnitDTO
        //                        {
        //                            Id = unit.Id,
        //                            Name = unit.Name,
        //                            Status = unit.Status,
        //                            FkProjectManagement = unit.FkProjectManagement,
        //                            ProjectManagement = new ProjectManagementDTO
        //                            {
        //                                Id = project.Id,
        //                                ProjectName = project.ProjectName,
        //                                CustomerName = project.CustomerName
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        );

        //        // ✅ Convert query to list and filter based on meterId
        //        var dataList = query.AsEnumerable()
        //            .Where(d => d.DeviceDataMaster?.Device?.FkUnitId != null &&
        //                        d.DeviceDataMaster?.FkDeviceId != null &&
        //                        meterId.ContainsKey(d.DeviceDataMaster.Device.FkUnitId.Value) &&
        //                        meterId[d.DeviceDataMaster.Device.FkUnitId.Value]
        //                            .Contains(d.DeviceDataMaster.FkDeviceId.Value))
        //            .ToList();

        //        // ✅ Ensure all requested meterId entries exist in the final result
        //        var result = new List<DeviceDataDetailDTO>(dataList);

        //        foreach (var unitEntry in meterId)
        //        {
        //            int unitKey = unitEntry.Key;
        //            foreach (var meter in unitEntry.Value)
        //            {
        //                bool exists = dataList.Any(d =>
        //                    d.DeviceDataMaster.Device.FkUnitId == unitKey &&
        //                    d.DeviceDataMaster.FkDeviceId == meter);

        //                if (!exists)
        //                {
        //                    result.Add(new DeviceDataDetailDTO
        //                    {
        //                        Id = 0,
        //                        FkDeviceDataMasterId = 0,
        //                        Address = "",
        //                        AddressVariable = 0.00,
        //                        CreatedAt = DateTime.MinValue,
        //                        DeviceDataMaster = new DeviceDataMasterDTO
        //                        {
        //                            Id = 0,
        //                            DeviceId = "",
        //                            CreatedAt = DateTime.MinValue,
        //                            FkDeviceId = meter,
        //                            Device = new DeviceDTO
        //                            {
        //                                Id = 0,
        //                                Name = "N/A",
        //                                SerialNo = "N/A",
        //                                Status = "N/A",
        //                                CreatedAt = DateTime.MinValue,
        //                                FkUnitId = unitKey,
        //                                Unit = new UnitDTO
        //                                {
        //                                    Id = unitKey,
        //                                    Name = "N/A",
        //                                    Status = "N/A",
        //                                    FkProjectManagement = 0,
        //                                    ProjectManagement = new ProjectManagementDTO
        //                                    {
        //                                        Id = 0,
        //                                        ProjectName = "N/A",
        //                                        CustomerName = "N/A"
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    });
        //                }
        //            }
        //        }

        //        // ✅ Apply Time Range Grouping
        //        return ApplyTimeRangeGrouping(result, timeRange);
        //    }

        // ✅ Helper Function for Time Grouping (LINQ Method Syntax)
        //private List<DeviceDataDetailDTO> ApplyTimeRangeGrouping(List<DeviceDataDetailDTO> data, string timeRange)
        //{
        //    if (string.IsNullOrEmpty(timeRange))
        //        return data;

        //    return timeRange.ToLower() switch
        //    {
        //        "yearly" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, d.Address })
        //                        .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
        //                        .ToList(),

        //        "monthly" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, Month = d.CreatedAt?.Month, d.Address })
        //                         .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
        //                         .ToList(),

        //        "daily" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, Month = d.CreatedAt?.Month, Day = d.CreatedAt?.Day, d.Address })
        //                       .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
        //                       .ToList(),

        //        "hourly" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, Month = d.CreatedAt?.Month, Day = d.CreatedAt?.Day, Hour = d.CreatedAt?.Hour, d.Address })
        //                        .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
        //                        .ToList(),

        //        "15minutes" => data.GroupBy(d => new {
        //            Year = d.CreatedAt?.Year,
        //            Month = d.CreatedAt?.Month,
        //            Day = d.CreatedAt?.Day,
        //            Hour = d.CreatedAt?.Hour,
        //            Quarter = d.CreatedAt?.Minute / 15,
        //            d.Address
        //        })
        //                           .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
        //                           .ToList(),

        //        _ => data // 🛑 Return unmodified data if time range is invalid
        //    };
        //}


        public async Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail2(ProjectDataRequest request)
        {
            // ✅ Extract values from request
            var projectIds = request.ProjectId != 0 ? new List<int> { request.ProjectId } : new List<int>();
            DateTime startDate = DateTime.ParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(request.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                                         .Date.AddDays(1).AddTicks(-1);
            string timeRange = request.TimeRange;

            // ✅ Build meterId dictionary from Units
            var meterId = request.Units
                .Where(u => u.UnitId != 0)
                .ToDictionary(
                    u => u.UnitId,
                    u => u.Meters.Where(m => m.MeterId != 0).Select(m => m.MeterId).ToList()
                );

            var validAddresses = new HashSet<string> { "EPI", "EPE" };
            //int EQL = request.Units[0].Meters[0].EQC;
            var query = (
                from detail in DBEMSContext.DeviceDataDetails
                join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
                join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id
                join unit in DBEMSContext.Units on device.FkUnitId equals unit.Id
                join project in DBEMSContext.ProjectManagements on unit.FkProjectManagement equals project.Id
                where projectIds.Contains(project.Id) &&
                      meterId.Keys.Contains(unit.Id) &&
                      detail.CreatedAt >= startDate &&
                      detail.CreatedAt <= endDate &&
                      validAddresses.Contains(detail.Address)
                select new DeviceDataDetailDTO
                {
                    Id = detail.Id,
                    FkDeviceDataMasterId = detail.FkDeviceDataMasterId,
                    Address = detail.Address,
                    AddressVariable = detail.AddressVariable,
                    CreatedAt = detail.CreatedAt,
                    DeviceDataMaster = new DeviceDataMasterDTO
                    {
                        Id = master.Id,
                        DeviceId = master.DeviceId,
                        CreatedAt = master.CreatedAt,
                        FkDeviceId = master.FkDeviceId,
                        Device = new DeviceDTO
                        {
                            Id = device.Id,
                            Name = device.Name,
                            SerialNo = device.SerialNo,
                            Status = device.Status,
                            CreatedAt = device.CreatedAt,
                            FkUnitId = device.FkUnitId,
                            Unit = new UnitDTO
                            {
                                Id = unit.Id,
                                Name = unit.Name,
                                Status = unit.Status,
                                FkProjectManagement = unit.FkProjectManagement,
                                ProjectManagement = new ProjectManagementDTO
                                {
                                    Id = project.Id,
                                    ProjectName = project.ProjectName,
                                    CustomerName = project.CustomerName
                                }
                            }
                        }
                    }
                }
            );

            // ✅ Convert query to list and filter based on meterId
            var dataList = query.AsEnumerable()
                .Where(d => d.DeviceDataMaster?.Device?.FkUnitId != null &&
                            d.DeviceDataMaster?.FkDeviceId != null &&
                            meterId.ContainsKey(d.DeviceDataMaster.Device.FkUnitId.Value) &&
                            meterId[d.DeviceDataMaster.Device.FkUnitId.Value]
                                .Contains(d.DeviceDataMaster.FkDeviceId.Value))
                .ToList();

            // ✅ Ensure all requested meterId entries exist in the final result
            var result = new List<DeviceDataDetailDTO>(dataList);

            foreach (var unitEntry in meterId)
            {
                int unitKey = unitEntry.Key;
                foreach (var meter in unitEntry.Value)
                {
                    bool exists = dataList.Any(d =>
                        d.DeviceDataMaster.Device.FkUnitId == unitKey &&
                        d.DeviceDataMaster.FkDeviceId == meter);

                    if (!exists)
                    {
                        result.Add(new DeviceDataDetailDTO
                        {
                            Id = 0,
                            FkDeviceDataMasterId = 0,
                            Address = "",
                            AddressVariable = 0.00,
                            CreatedAt = DateTime.MinValue,
                            DeviceDataMaster = new DeviceDataMasterDTO
                            {
                                Id = 0,
                                DeviceId = "",
                                CreatedAt = DateTime.MinValue,
                                FkDeviceId = meter,
                                Device = new DeviceDTO
                                {
                                    Id = 0,
                                    Name = "N/A",
                                    SerialNo = "N/A",
                                    Status = "N/A",
                                    CreatedAt = DateTime.MinValue,
                                    FkUnitId = unitKey,
                                    Unit = new UnitDTO
                                    {
                                        Id = unitKey,
                                        Name = "N/A",
                                        Status = "N/A",
                                        FkProjectManagement = 0,
                                        ProjectManagement = new ProjectManagementDTO
                                        {
                                            Id = 0,
                                            ProjectName = "N/A",
                                            CustomerName = "N/A"
                                        }
                                    }
                                }
                            }
                        });
                    }
                }
            }

            // ✅ Apply Time Range Grouping
            return ApplyTimeRangeGrouping(result, timeRange);
        }
        
        private DateTime GetTimeGrouping(DateTime? createdAt, string timeRange)
        {
            if (!createdAt.HasValue)
                return DateTime.MinValue;

            DateTime date = createdAt.Value;

            return timeRange.ToLower() switch
            {
                "15minutes" => new DateTime(date.Year, date.Month, date.Day, date.Hour, (date.Minute / 15) * 15, 0),
                "hourly" => new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0),
                "daily" => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0),
                "weekly" => date.AddDays(-(int)date.DayOfWeek).Date,
                "monthly" => new DateTime(date.Year, date.Month, 1),
                "quarterly" => new DateTime(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1),
                "yearly" => new DateTime(date.Year, 1, 1),
                _ => date // Default: No grouping
            };
        }

      public async Task<List<UnitWiseAddressVariableSumDTO>> Getpowerloadtoday()
{
            DateTime start = DateTime.Today;               // 2025-04-04 00:00:00
            DateTime end = start.AddDays(1);               // 2025-04-05 00:00:00 (exclusive end)


            var result = await (
                from detail in DBEMSContext.DeviceDataDetails
                join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
                join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id
                join unit in DBEMSContext.Units on device.FkUnitId equals unit.Id
                where detail.Address == "P"
  //&& detail.CreatedAt >= start
  //&& detail.CreatedAt < end

                group detail by new
                {
                    UnitId = unit.Id,
                    UnitName = unit.Name
                } into unitGroup
                select new UnitWiseAddressVariableSumDTO
                {
                    UnitId = unitGroup.Key.UnitId,
                    UnitName = unitGroup.Key.UnitName,
                    TotalAddressVariable = unitGroup.Sum(d => d.AddressVariable)
                }
            ).ToListAsync();


            return result;
}

        public async Task<List<HourlyAddressVariableSumDTO>> GetPowerLoadTodayHourly()
        {
            // Specify the target time range (4:00 PM to 4:10 PM)
            DateTime targetStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 16, 0, 0);  // 4:00 PM
            DateTime targetEndTime = targetStartTime.AddMinutes(10); // 4:10 PM

            // Step 1: Pull all data for the specified time range
            var rawData = await (
                from detail in DBEMSContext.DeviceDataDetails
                join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
                join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id
                join unit in DBEMSContext.Units on device.FkUnitId equals unit.Id
                where detail.Address == "EPI"
                    && detail.CreatedAt >= targetStartTime
                    && detail.CreatedAt <= targetEndTime
                select new
                {
                    CreatedAt = detail.CreatedAt.Value,
                    detail.AddressVariable,
                    UnitId = unit.Id,
                    UnitName = unit.Name
                }
            ).ToListAsync();

            // Step 2: Group by minute and unit
            var result = rawData
                .GroupBy(x => new
                {
                    MinuteBlock = x.CreatedAt.Minute,  // Group by minute (0-59)
                    x.UnitId,
                    x.UnitName
                })
                .OrderBy(g => g.Key.MinuteBlock)  // Sort by minute block (ascending)
                .Select(g => new HourlyAddressVariableSumDTO
                {
                    MinuteBlock = g.Key.MinuteBlock,
                    UnitId = g.Key.UnitId,
                    UnitName = g.Key.UnitName,
                    TotalAddressVariable = g.Sum(x => x.AddressVariable)  // Sum AddressVariable for each minute block
                })
                .ToList();

            return result;
        }

        public async Task<List<RealTimeDataDeviceUnitWise>> GetPowerConsumptionRealtime()
        {
            DateTime endTime = DateTime.Now;
            DateTime startTime = endTime.AddMinutes(-1); // Last 1 minute

            // Step 1: Fetch and group data in one query directly from the database
            var rawData = await (
                from detail in DBEMSContext.DeviceDataDetails
                join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
                join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id
                join unit in DBEMSContext.Units on device.FkUnitId equals unit.Id
                where detail.Address == "P"
                    && detail.CreatedAt >= startTime
                    && detail.CreatedAt <= endTime
                group detail by new
                {
                    MinuteBlock = detail.CreatedAt.Value.Date, // Group by DateTime value directly
                    UnitId = unit.Id, // Unique name for UnitId
                    UnitName = unit.Name, // Unique name for UnitName
                    DeviceId = device.Id, // Unique name for DeviceId
                    DeviceName = device.Name // Unique name for DeviceName
                } into g
                select new RealTimeDataDeviceUnitWise
                {
                    MinuteBlockString = g.Key.MinuteBlock.ToString("yyyy-MM-dd HH:mm"), // Format DateTime after retrieval
                    UnitId = g.Key.UnitId, // Use distinct name for UnitId
                    UnitName = g.Key.UnitName, // Use distinct name for UnitName
                    DeviceId = g.Key.DeviceId, // Use distinct name for DeviceId
                    DeviceName = g.Key.DeviceName, // Use distinct name for DeviceName
                    TotalAddressVariable = g.Sum(x => x.AddressVariable) // Aggregate data
                }
            ).ToListAsync(); // Retrieve and group data in one step

            // Step 2: Return the result sorted by UnitId and DeviceId
            return rawData.OrderBy(x => x.UnitId)
                          .ThenBy(x => x.DeviceId)
                          .ToList();
        }


        public async Task<List<UnitWiseAddressVariableSumDTO>> Getpowerloadhourly()
        {
            DateTime start = DateTime.Today.AddHours(DateTime.Now.Hour);
            DateTime end = start.AddHours(1);

            var result = await (
                from detail in DBEMSContext.DeviceDataDetails
                join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
                join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id
                join unit in DBEMSContext.Units on device.FkUnitId equals unit.Id
                where detail.Address == "P"
                      //&& detail.CreatedAt >= start
                      //&& detail.CreatedAt < end
                group detail by new
                {
                    UnitId = unit.Id,
                    UnitName = unit.Name
                } into unitGroup
                select new UnitWiseAddressVariableSumDTO
                {
                    UnitId = unitGroup.Key.UnitId,
                    UnitName = unitGroup.Key.UnitName,
                    TotalAddressVariable = unitGroup.Sum(d => d.AddressVariable),
                    Date = start // ⏱ optional: include the hour being reported
                }
            ).ToListAsync();

            return result;
        }






        // ✅ Helper Function for Time Grouping (LINQ Method Syntax)
        private List<DeviceDataDetailDTO> ApplyTimeRangeGrouping(List<DeviceDataDetailDTO> data, string timeRange)
        {
            if (string.IsNullOrEmpty(timeRange))
                return data;

            return timeRange.ToLower() switch
            {
                "yearly" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, d.Address })
                                .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
                                .ToList(),

                "monthly" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, Month = d.CreatedAt?.Month, d.Address })
                                 .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
                                 .ToList(),

                "daily" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, Month = d.CreatedAt?.Month, Day = d.CreatedAt?.Day, d.Address })
                               .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
                               .ToList(),

                "hourly" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, Month = d.CreatedAt?.Month, Day = d.CreatedAt?.Day, Hour = d.CreatedAt?.Hour, d.Address })
                                .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
                                .ToList(),

                "15minutes" => data.GroupBy(d => new {
                    Year = d.CreatedAt?.Year,
                    Month = d.CreatedAt?.Month,
                    Day = d.CreatedAt?.Day,
                    Hour = d.CreatedAt?.Hour,
                    Quarter = d.CreatedAt?.Minute / 15,
                    d.Address
                })
                                   .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault())
                                   .ToList(),

                _ => data // 🛑 Return unmodified data if time range is invalid
            };
        }

        public Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail(IEnumerable<int> projectId, IEnumerable<int> unitId, Dictionary<int, List<int>> meterId, DateTime startDate, DateTime endDate, string timeRange)
        {
            throw new NotImplementedException();
        }
    }

}
