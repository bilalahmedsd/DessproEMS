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

        // EPI CHART DATA
        public async Task<EPIConspDTO> GetEPIConspAsync(int deviceId, string range)
        {
            DateTime now = DateTime.UtcNow.Date;
            DateTime start, end;

            if (range == "weekly")
            {
                end = now.AddDays(0); // Today
                start = end.AddDays(-6); // 6 days before yesterday = 7-day total range
            }
            else // monthly
            {
                start = new DateTime(now.Year, now.Month, 1);
                end = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59);
            }

            var deviceMasterIds = await DBEMSContext.DeviceDataMasters
                .Where(d => d.FkDeviceId == deviceId)
                .Select(d => d.Id).AsQueryable()
                .ToListAsync();

            if (!deviceMasterIds.Any())
                return null;

            double previousValue = await DBEMSContext.DeviceDataDetails
                .Where(d => deviceMasterIds.Contains((int)d.FkDeviceDataMasterId) &&
                            d.CreatedAt < start &&
                            d.Address == "EPI")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => (double?)d.AddressVariable)
                .FirstOrDefaultAsync() ?? 0;

            double latestValue = await DBEMSContext.DeviceDataDetails
                .Where(d => deviceMasterIds.Contains((int)d.FkDeviceDataMasterId) &&
                            d.CreatedAt >= start &&
                            d.CreatedAt <= end &&
                            d.Address == "EPI")
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => (double?)d.AddressVariable)
                .FirstOrDefaultAsync() ?? 0;

            double totalConsumed = (latestValue - previousValue) ;

            List<EPIConspData> result;

            if (range.ToLower() == "weekly")
            {
                var datesToFetch = Enumerable.Range(-1, 8)
                    .Select(i => start.AddDays(i).Date).AsQueryable()
                    .ToList();

                var weeklyData = await DBEMSContext.DeviceDataDetails
                    .Where(d => deviceMasterIds.Contains((int)d.FkDeviceDataMasterId) &&
                                d.CreatedAt.HasValue &&
                                datesToFetch.Contains(d.CreatedAt.Value.Date) &&
                                d.Address == "EPI").AsQueryable()
                    .ToListAsync();

                result = Enumerable.Range(0, 7)
                    .Select(i =>
                    {
                        var currentDay = start.AddDays(i).Date;
                        var previousDay = currentDay.AddDays(-1);
                        var previousValueDay = 0.00;

                        var currentValue = weeklyData
                            .Where(d => d.CreatedAt.Value.Date == currentDay).AsQueryable()
                            .OrderByDescending(d => d.CreatedAt.Value)
                            .Select(d => (double?)d.AddressVariable)
                            .FirstOrDefault() ?? 0;
                        for (global::System.Int32 j = 1; j <= 7; j++)
                        {
                            var tempPrev = weeklyData
                            .Where(d => d.CreatedAt.Value.Date == currentDay.AddDays(-j))
                            .OrderByDescending(d => d.CreatedAt.Value)
                            .Select(d => (double?)d.AddressVariable)
                            .FirstOrDefault();

                            if (tempPrev.HasValue)
                            {
                                previousValueDay = tempPrev.Value;
                                break;
                            }
                        }



                        var safeDiff = Math.Max(currentValue - previousValueDay, 0);

                        return new EPIConspData
                        {
                            Name = currentDay.DayOfWeek.ToString(), // e.g., "Sunday"
                            Value = Math.Round(safeDiff , 2),
                            CreatedAt = currentDay.AddDays(1).AddMilliseconds(-1)
                        };
                    })
                    .ToList();
            }
            else // monthly
            {
                result = new List<EPIConspData>();

                for (int month = 1; month <= 12; month++)
                {
                    var firstDay = new DateTime(now.Year, month, 1);
                    var lastDay = new DateTime(now.Year, month, DateTime.DaysInMonth(now.Year, month), 23, 59, 59); // Last Day


                    double prevValue = await DBEMSContext.DeviceDataDetails
                        .Where(d => deviceMasterIds.Contains((int)d.FkDeviceDataMasterId) &&
                                    d.CreatedAt.HasValue &&
                                    d.CreatedAt.Value < firstDay &&
                                    d.Address == "EPI")
                        .OrderByDescending(d => d.CreatedAt.Value)
                        .Select(d => (double?)d.AddressVariable)
                        .FirstOrDefaultAsync() ?? 0;

                    double currValue = await DBEMSContext.DeviceDataDetails
                        .Where(d => deviceMasterIds.Contains((int)d.FkDeviceDataMasterId) &&
                                    d.CreatedAt.HasValue &&
                                    d.CreatedAt.Value >= firstDay &&
                                    d.CreatedAt.Value <= lastDay &&
                                    d.Address == "EPI")
                        .OrderByDescending(d => d.CreatedAt.Value)
                        .Select(d => (double?)d.AddressVariable)
                        .FirstOrDefaultAsync() ?? 0;

                    double safeDiff = Math.Max(currValue - prevValue, 0);

                    result.Add(new EPIConspData
                    {
                        Name = firstDay.ToString("MMMM"),
                        Value = Math.Round(safeDiff , 2),
                        CreatedAt = lastDay
                    });
                }
            }

            return new EPIConspDTO
            {
                UnitId = deviceId,
                Data = result,
                CreatedAt = DateTime.UtcNow
            };
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
                }).AsQueryable()
                .ToListAsync();

            // Multiply AddressVariable by 0.06 and group data into 15-minute slots
            var adjustedData = data.Select((d, index) => new
            {
                d.CreatedAt,
                AdjustedAddressVariable = d.AddressVariable 
            }).AsQueryable()
 .OrderBy(d => d.CreatedAt)
 .Where((item, index) => index % 2 == 0) // This filters to get odd indexed items (0, 2, 4, 6, etc.)
 .ToList();


            // Calculate the difference in AdjustedAddressVariable between consecutive slots
            var groupedData = timeSlots.Select(slot =>
            {
                var slotEnd = slot.Add(slotDuration);

                // Get the data for this 15-minute slot
                var dataInSlot = adjustedData.Where(d => d.CreatedAt >= slot && d.CreatedAt < slotEnd).AsQueryable().ToList();

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
            }).Where(d => d != null).AsQueryable().ToList();

            return groupedData;


        }

        public async Task<List<PowerLoadDTO>> GetEnergyConspDatau1()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddHours(-12); // Last 12 hours
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id).AsQueryable()
                    .ToListAsync();

                if (deviceIds.Any())
                {
                    // Preload all DeviceDataMasters for these devices
                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
        .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value)
        && dm.CreatedAt >= startTime && dm.CreatedAt <= endTime
        )
        .Select(dm => new { dm.Id, dm.FkDeviceId }).AsQueryable()
        .ToListAsync();


                    // Preload all DeviceDataDetails for EPI address and time range
                    var allDeviceData = await DBEMSContext.DeviceDataDetails
                        .Where(d =>
                            d.FkDeviceDataMasterId.HasValue &&
                            dataMasterIds.Select(x => x.Id).Contains(d.FkDeviceDataMasterId.Value) &&
                            d.Address == "EPI" &&
                            d.CreatedAt >= startTime &&
                            d.CreatedAt <= endTime
                        ).AsQueryable()
                        .OrderBy(d => d.CreatedAt)
                        .ToListAsync();

                    int totalHours = (int)(endTime - startTime).TotalHours;


                    // Grouping by each hour
                    for (int i = 0; i < totalHours; i++)
                    {
                        var hourStart = startTime.AddHours(i);
                        var hourEnd = hourStart.AddHours(1);

                        double? totalDifference = 0;

                        foreach (var deviceId in deviceIds)
                        {
                            var currentDeviceMasterIds = dataMasterIds
                                .Where(dm => dm.FkDeviceId == deviceId)
                                .Select(dm => dm.Id).AsQueryable()
                                .ToList();

                            var deviceData = allDeviceData
                                .Where(d =>
                                    currentDeviceMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                    d.CreatedAt >= hourStart &&
                                    d.CreatedAt < hourEnd
                                ).AsQueryable()
                                .OrderBy(d => d.CreatedAt)
                                .ToList();

                            if (deviceData.Any())
                            {
                                var first = deviceData.FirstOrDefault();
                                var last = deviceData.LastOrDefault();

                                if (first != null && last != null)
                                {
                                    var deviceDiff = last.AddressVariable - first.AddressVariable;
                                    totalDifference += deviceDiff;

                                }
                            }
                        }

                        result.Add(new PowerLoadDTO
                        {
                            UnitName = unit.Name,
                            AddressVariable = totalDifference ,
                            CreatedAt = hourStart // Represent the start of the hour
                        });


                    }
                }
            }

            return result;
        }


        public async Task<List<PowerLoadDTO>> GetEnergyConspDatau2()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddHours(-12); // Last 12 hours
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(1).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id).AsQueryable()
                    .ToListAsync();

                if (deviceIds.Any())
                {
                    // Preload all DeviceDataMasters for these devices
                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
        .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value)
        && dm.CreatedAt >= startTime && dm.CreatedAt <= endTime
        )
        .Select(dm => new { dm.Id, dm.FkDeviceId }).AsQueryable()
        .ToListAsync();


                    // Preload all DeviceDataDetails for EPI address and time range
                    var allDeviceData = await DBEMSContext.DeviceDataDetails
                        .Where(d =>
                            d.FkDeviceDataMasterId.HasValue &&
                            dataMasterIds.Select(x => x.Id).Contains(d.FkDeviceDataMasterId.Value) &&
                            d.Address == "EPI" &&
                            d.CreatedAt >= startTime &&
                            d.CreatedAt <= endTime
                        ).AsQueryable()
                        .OrderBy(d => d.CreatedAt)
                        .ToListAsync();

                    int totalHours = (int)(endTime - startTime).TotalHours;


                    // Grouping by each hour
                    for (int i = 0; i < totalHours; i++)
                    {
                        var hourStart = startTime.AddHours(i);
                        var hourEnd = hourStart.AddHours(1);

                        double? totalDifference = 0;

                        foreach (var deviceId in deviceIds)
                        {
                            var currentDeviceMasterIds = dataMasterIds
                                .Where(dm => dm.FkDeviceId == deviceId)
                                .Select(dm => dm.Id).AsQueryable()
                                .ToList();

                            var deviceData = allDeviceData
                                .Where(d =>
                                    currentDeviceMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                    d.CreatedAt >= hourStart &&
                                    d.CreatedAt < hourEnd
                                ).AsQueryable()
                                .OrderBy(d => d.CreatedAt)
                                .ToList();

                            if (deviceData.Any())
                            {
                                var first = deviceData.FirstOrDefault();
                                var last = deviceData.LastOrDefault();

                                if (first != null && last != null)
                                {
                                    var deviceDiff = last.AddressVariable - first.AddressVariable;
                                    totalDifference += deviceDiff;

                                }
                            }
                        }

                        result.Add(new PowerLoadDTO
                        {
                            UnitName = unit.Name,
                            AddressVariable = totalDifference ,
                            CreatedAt = hourStart // Represent the start of the hour
                        });


                    }
                }
            }

            return result;

        }


        public async Task<List<PowerLoadDTO>> GetEnergyConspDatau3()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddHours(-12); // Last 12 hours
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(2).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id).AsQueryable()
                    .ToListAsync();

                if (deviceIds.Any())
                {
                    // Preload all DeviceDataMasters for these devices
                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
        .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value)
        && dm.CreatedAt >= startTime && dm.CreatedAt <= endTime
        )
        .Select(dm => new { dm.Id, dm.FkDeviceId }).AsQueryable()
        .ToListAsync();


                    // Preload all DeviceDataDetails for EPI address and time range
                    var allDeviceData = await DBEMSContext.DeviceDataDetails
                        .Where(d =>
                            d.FkDeviceDataMasterId.HasValue &&
                            dataMasterIds.Select(x => x.Id).Contains(d.FkDeviceDataMasterId.Value) &&
                            d.Address == "EPI" &&
                            d.CreatedAt >= startTime &&
                            d.CreatedAt <= endTime
                        ).AsQueryable()
                        .OrderBy(d => d.CreatedAt)
                        .ToListAsync();

                    int totalHours = (int)(endTime - startTime).TotalHours;


                    // Grouping by each hour
                    for (int i = 0; i < totalHours; i++)
                    {
                        var hourStart = startTime.AddHours(i);
                        var hourEnd = hourStart.AddHours(1);

                        double? totalDifference = 0;

                        foreach (var deviceId in deviceIds)
                        {
                            var currentDeviceMasterIds = dataMasterIds
                                .Where(dm => dm.FkDeviceId == deviceId)
                                .Select(dm => dm.Id).AsQueryable()
                                .ToList();

                            var deviceData = allDeviceData
                                .Where(d =>
                                    currentDeviceMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                    d.CreatedAt >= hourStart &&
                                    d.CreatedAt < hourEnd
                                ).AsQueryable()
                                .OrderBy(d => d.CreatedAt)
                                .ToList();

                            if (deviceData.Any())
                            {
                                var first = deviceData.FirstOrDefault();
                                var last = deviceData.LastOrDefault();

                                if (first != null && last != null)
                                {
                                    var deviceDiff = last.AddressVariable - first.AddressVariable;
                                    totalDifference += deviceDiff;

                                }
                            }
                        }

                        result.Add(new PowerLoadDTO
                        {
                            UnitName = unit.Name,
                            AddressVariable = totalDifference ,
                            CreatedAt = hourStart // Represent the start of the hour
                        });


                    }
                }
            }

            return result;
        }

        public async Task<List<PowerLoadDTO>> GetPowerLoadu1()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-11);
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.AsQueryable().OrderBy(u => u.Id).FirstOrDefaultAsync();
            if (unit == null) return result;

            var deviceIds = await DBEMSContext.Devices
                .Where(dev => dev.FkUnitId == unit.Id)
                .Select(dev => dev.Id).AsQueryable()
                .ToListAsync();
            if (!deviceIds.Any()) return result;

            var dataMasterMap = await DBEMSContext.DeviceDataMasters
                .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                .Select(dm => new { dm.Id, DeviceId = dm.FkDeviceId.Value }).AsQueryable()
                .ToListAsync();
            if (!dataMasterMap.Any()) return result;

            var dataMasterIds = dataMasterMap.Select(dm => dm.Id).ToList();

            var rawData = await DBEMSContext.DeviceDataDetails
                .Where(d =>
                    d.FkDeviceDataMasterId.HasValue &&
                    dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                    d.Address == "P" &&
                    d.CreatedAt.HasValue &&
                    d.CreatedAt >= startTime &&
                    d.CreatedAt <= endTime)
                .Select(d => new
                {
                    d.FkDeviceDataMasterId,
                    d.AddressVariable,
                    d.CreatedAt
                }).AsQueryable()
                .ToListAsync();

            // Group raw data by minute and device
            var groupedData = rawData
                .GroupBy(d => new
                {
                    Minute = new DateTime(d.CreatedAt.Value.Year, d.CreatedAt.Value.Month, d.CreatedAt.Value.Day, d.CreatedAt.Value.Hour, d.CreatedAt.Value.Minute, 0),
                    DeviceId = dataMasterMap.First(dm => dm.Id == d.FkDeviceDataMasterId.Value).DeviceId
                })
                .Select(g => new
                {
                    g.Key.Minute,
                    g.Key.DeviceId,
                    Value = g.OrderByDescending(x => x.CreatedAt).First().AddressVariable ?? 0
                }).AsQueryable()
                .ToList();

            var minutes = Enumerable.Range(0, (int)(endTime - startTime).TotalMinutes + 1)
                .Select(i => startTime.AddMinutes(i)).AsQueryable()
                .ToList();

            foreach (var minute in minutes)
            {
                double sum = groupedData
                    .Where(d => d.Minute == minute).AsQueryable()
                    .Sum(d => d.Value);

                result.Add(new PowerLoadDTO
                {
                    UnitName = unit.Name,
                    AddressVariable = sum,
                    CreatedAt = minute
                });
            }

            return result;
        }




        public async Task<List<PowerLoadDTO>> LoadProfile()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.Date.AddDays(-6); // Last 7 days including today
            var result = new List<PowerLoadDTO>();

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
                        var rawData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "P" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime)
                            .OrderBy(d => d.CreatedAt)
                            .ToListAsync();

                        // 🔸 Generate all days in range
                        var allDays = Enumerable.Range(0, 7)
                            .Select(i => startTime.AddDays(i).Date)
                            .ToList();

                        foreach (var day in allDays)
                        {
                            var dayData = rawData
                                .Where(d => d.CreatedAt.Value.Date == day && d.AddressVariable.HasValue)
                                .Select(d => d.AddressVariable.Value)
                                .ToList();

                            double avgPower = 0;
                            if (dayData.Any())
                            {
                                avgPower = dayData.Average(); // Assuming value is *10
                            }

                            result.Add(new PowerLoadDTO
                            {
                                UnitName = unit.Name,
                                AddressVariable = avgPower,
                                CreatedAt = day
                            });
                        }
                    }
                }
            }

            return result;
        }


        public async Task<List<PowerLoadDTO>> LoadProfilev1()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.Date.AddDays(-6);
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.AsQueryable().OrderBy(u => u.Id).Skip(1).FirstOrDefaultAsync();

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
                        var rawData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "P" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime)
                            .OrderBy(d => d.CreatedAt)
                            .ToListAsync();

                        var dailyGroups = rawData
                            .GroupBy(d => d.CreatedAt.Value.Date)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        // Ensure all 7 days are represented
                        for (int i = 0; i < 7; i++)
                        {
                            var currentDate = startTime.AddDays(i);

                            if (dailyGroups.ContainsKey(currentDate))
                            {
                                var group = dailyGroups[currentDate];
                                var validReadings = group
                                    .Where(g => g.AddressVariable.HasValue)
                                    .Select(g => g.AddressVariable.Value)
                                    .ToList();

                                var avgPower = validReadings.Any() ? validReadings.Average()  : 0;

                                result.Add(new PowerLoadDTO
                                {
                                    UnitName = unit.Name,
                                    AddressVariable = avgPower,
                                    CreatedAt = currentDate
                                });
                            }
                            else
                            {
                                // No data for this day, add zero
                                result.Add(new PowerLoadDTO
                                {
                                    UnitName = unit.Name,
                                    AddressVariable = 0,
                                    CreatedAt = currentDate
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }



        public async Task<List<PowerLoadDTO>> LoadProfilev2()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.Date.AddDays(-6); // Last 7 days including today
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.AsQueryable().OrderBy(u => u.Id).Skip(2).FirstOrDefaultAsync();

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
                        var rawData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "P" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime)
                            .OrderBy(d => d.CreatedAt)
                            .ToListAsync();

                        var dailyGroups = rawData
                            .GroupBy(d => d.CreatedAt.Value.Date)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        for (int i = 0; i < 7; i++)
                        {
                            var currentDate = startTime.AddDays(i);

                            if (dailyGroups.ContainsKey(currentDate))
                            {
                                var group = dailyGroups[currentDate];
                                var validReadings = group
                                    .Where(g => g.AddressVariable.HasValue)
                                    .Select(g => g.AddressVariable.Value)
                                    .ToList();
                                
                                var avgPower = validReadings.Any() ? validReadings.Average() : 0;


                                result.Add(new PowerLoadDTO
                                {
                                    UnitName = unit.Name,
                                    AddressVariable = avgPower,
                                    CreatedAt = currentDate
                                });
                            }
                            else
                            {
                                // No data for this day, insert 0
                                result.Add(new PowerLoadDTO
                                {
                                    UnitName = unit.Name,
                                    AddressVariable = 0,
                                    CreatedAt = currentDate
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }


        public async Task<List<ConsumptionDetailsDTO>> GetConsumptions(
       DateTime selectedDate,
       int? selectedUnit,
       int? selectedDevices,
       string selectedRange)
        {
            var result = new List<ConsumptionDetailsDTO>();
            var selectedDateTime = selectedDate;
            List<DeviceDataDetail> allDeviceData = new();
            var addressList = new[] { "EPI", "EQL", "EQC", "EPE" };

            if (selectedUnit != null)
            {
                switch (selectedRange?.ToLower())
                {
                    case "date":
                        {
                            var previousDate = selectedDateTime.Date.AddDays(-1);  // <-- Declare once here

                            var deviceIds = await DBEMSContext.Devices
                                .Where(dev => dev.FkUnitId == selectedUnit.Value)
                                .Select(dev => dev.Id).AsQueryable()
                                .ToListAsync();

                            if (deviceIds.Any())
                            {
                                var dataMasterIds = await DBEMSContext.DeviceDataMasters
                                    .Where(dm => deviceIds.Contains(dm.FkDeviceId.Value) &&
                                                 dm.CreatedAt.HasValue &&
                                                 (dm.CreatedAt.Value.Date == selectedDateTime.Date ||
                                                  dm.CreatedAt.Value.Date == previousDate))
                                    .Select(dm => dm.Id).AsQueryable()
                                    .ToListAsync();

                                if (dataMasterIds.Any())
                                {
                                    allDeviceData = await DBEMSContext.DeviceDataDetails
                                        .Where(d =>
                                            d.FkDeviceDataMasterId.HasValue &&
                                            dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                            (
                                                (d.CreatedAt.Value.Date == previousDate &&
                                                 (d.Address == "EPI" || d.Address == "EPE")) ||  // <-- EPI and EPE for previous date
                                                (d.CreatedAt.Value.Date == selectedDateTime.Date &&
                                                 addressList.Contains(d.Address))  // <-- all selected addresses for current date
                                            ) &&
                                            d.CreatedAt.HasValue)
                                        .AsQueryable()
                                        .OrderBy(d => d.CreatedAt)
                                        .ToListAsync();
                                }
                            }

                            // Group by Date, Hour and Address to separate previous and selected date data
                            var hourlyGroups = allDeviceData
                                .GroupBy(d => new { Date = d.CreatedAt.Value.Date, Hour = d.CreatedAt.Value.Hour, d.Address });

                            var hourlyTotals = new Dictionary<(DateTime Date, int Hour, string Address), double>();

                            foreach (var group in hourlyGroups)
                            {
                                var first = group.FirstOrDefault();
                                var last = group.LastOrDefault();

                                if (first != null && last != null)
                                {
                                    var diff = (last.AddressVariable ?? 0) - (first.AddressVariable ?? 0);
                                    var key = (group.Key.Date, group.Key.Hour, group.Key.Address);

                                    if (hourlyTotals.ContainsKey(key))
                                    {
                                        hourlyTotals[key] += diff;
                                    }
                                    else
                                    {
                                        hourlyTotals[key] = diff ;
                                    }
                                }
                            }

                            // Prepare results for previousDate for EPI and EPE
                            foreach (var address in new[] { "EPI", "EPE" })
                            {
                                for (int hour = 0; hour < 24; hour++)
                                {
                                    result.Add(new ConsumptionDetailsDTO
                                    {
                                        SelectedDate = previousDate.AddHours(hour),
                                        SelectedUnit = selectedUnit.Value,
                                        SelectedDevice = selectedDevices,
                                        SelectedRange = selectedRange,
                                        Address = address,
                                        AddressVariable = hourlyTotals.TryGetValue((previousDate, hour, address), out var val) ? val : 0.0
                                    });
                                }
                            }

                            // Prepare results for selectedDate for all selected addresses
                            foreach (var address in addressList)
                            {
                                for (int hour = 0; hour < 24; hour++)
                                {
                                    result.Add(new ConsumptionDetailsDTO
                                    {
                                        SelectedDate = selectedDateTime.Date.AddHours(hour),
                                        SelectedUnit = selectedUnit.Value,
                                        SelectedDevice = selectedDevices,
                                        SelectedRange = selectedRange,
                                        Address = address,
                                        AddressVariable = hourlyTotals.TryGetValue((selectedDateTime.Date, hour, address), out var val) ? val : 0.0
                                    });
                                }
                            }

                            break;
                        }

                    case "week":
                        {
                            DateTime weekStart = selectedDateTime.Date.AddDays(-(int)selectedDateTime.DayOfWeek);
                            DateTime weekEnd = weekStart.AddDays(7);

                            DateTime prevWeekStart = weekStart.AddDays(-7);
                            DateTime prevWeekEnd = weekStart;

                            var deviceIds = await DBEMSContext.Devices
                                .Where(dev => dev.FkUnitId == selectedUnit.Value)
                                .Select(dev => dev.Id).AsQueryable()
                                .ToListAsync();

                            if (deviceIds.Any())
                            {
                                var dataMasterIds = await DBEMSContext.DeviceDataMasters
                                    .Where(dm => deviceIds.Contains(dm.FkDeviceId.Value) &&
                                                 dm.CreatedAt.HasValue &&
                                                 ((dm.CreatedAt.Value.Date >= weekStart && dm.CreatedAt.Value.Date < weekEnd) ||
                                                  (dm.CreatedAt.Value.Date >= prevWeekStart && dm.CreatedAt.Value.Date < prevWeekEnd)))
                                    .Select(dm => dm.Id).AsQueryable()
                                    .ToListAsync();

                                if (dataMasterIds.Any())
                                {
                                    allDeviceData = await DBEMSContext.DeviceDataDetails
                                        .Where(d =>
                                            d.FkDeviceDataMasterId.HasValue &&
                                            dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                            (
                                                (d.CreatedAt.Value.Date >= prevWeekStart && d.CreatedAt.Value.Date < prevWeekEnd &&
                                                 (d.Address == "EPI" || d.Address == "EPE")) ||  // <-- Previous week: EPI and EPE
                                                (d.CreatedAt.Value.Date >= weekStart && d.CreatedAt.Value.Date < weekEnd &&
                                                 addressList.Contains(d.Address))  // <-- Current week: all selected addresses
                                            ) &&
                                            d.CreatedAt.HasValue)
                                        .OrderBy(d => d.CreatedAt)
                                        .AsQueryable()
                                        .ToListAsync();
                                }
                            }

                            var dailyGroups = allDeviceData
                                .GroupBy(d => new { Day = d.CreatedAt.Value.Date, d.Address });

                            var dailyTotals = new Dictionary<(DateTime Day, string Address), double>();

                            foreach (var group in dailyGroups)
                            {
                                var first = group.FirstOrDefault();
                                var last = group.LastOrDefault();

                                if (first != null && last != null)
                                {
                                    var diff = (last.AddressVariable ?? 0) - (first.AddressVariable ?? 0);
                                    dailyTotals[(group.Key.Day, group.Key.Address)] = diff ;
                                }
                            }

                            // Previous week: EPI and EPE
                            foreach (var address in new[] { "EPI", "EPE" })
                            {
                                for (int day = 0; day < 7; day++)
                                {
                                    var currentDate = prevWeekStart.AddDays(day);
                                    result.Add(new ConsumptionDetailsDTO
                                    {
                                        SelectedDate = currentDate,
                                        SelectedUnit = selectedUnit.Value,
                                        SelectedDevice = selectedDevices,
                                        SelectedRange = selectedRange,
                                        Address = address,
                                        AddressVariable = dailyTotals.TryGetValue((currentDate, address), out var val) ? val : 0.0
                                    });
                                }
                            }

                            // Current week: all selected addresses
                            for (int day = 0; day < 7; day++)
                            {
                                var currentDate = weekStart.AddDays(day);
                                foreach (var address in addressList)
                                {
                                    result.Add(new ConsumptionDetailsDTO
                                    {
                                        SelectedDate = currentDate,
                                        SelectedUnit = selectedUnit.Value,
                                        SelectedDevice = selectedDevices,
                                        SelectedRange = selectedRange,
                                        Address = address,
                                        AddressVariable = dailyTotals.TryGetValue((currentDate, address), out var val) ? val : 0.0
                                    });
                                }
                            }

                            break;
                        }


                    case "month":
                        {
                            DateTime monthStart = new DateTime(selectedDateTime.Year, selectedDateTime.Month, 1);
                            DateTime monthEnd = monthStart.AddMonths(1);
                            int daysInMonth = DateTime.DaysInMonth(selectedDateTime.Year, selectedDateTime.Month);

                            DateTime prevMonthStart = monthStart.AddMonths(-1);
                            DateTime prevMonthEnd = monthStart;
                            int daysInPrevMonth = DateTime.DaysInMonth(prevMonthStart.Year, prevMonthStart.Month);

                            var deviceIds = await DBEMSContext.Devices
                                .Where(dev => dev.FkUnitId == selectedUnit.Value)
                                .Select(dev => dev.Id).AsQueryable()
                                .ToListAsync();

                            if (deviceIds.Any())
                            {
                                var dataMasterIds = await DBEMSContext.DeviceDataMasters
                                    .Where(dm => deviceIds.Contains(dm.FkDeviceId.Value) &&
                                                 dm.CreatedAt.HasValue &&
                                                 ((dm.CreatedAt.Value.Date >= monthStart && dm.CreatedAt.Value.Date < monthEnd) ||
                                                  (dm.CreatedAt.Value.Date >= prevMonthStart && dm.CreatedAt.Value.Date < prevMonthEnd)))
                                    .Select(dm => dm.Id).AsQueryable()
                                    .ToListAsync();

                                if (dataMasterIds.Any())
                                {
                                    allDeviceData = await DBEMSContext.DeviceDataDetails
                                        .Where(d =>
                                            d.FkDeviceDataMasterId.HasValue &&
                                            dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                            (
                                                (d.CreatedAt.Value.Date >= prevMonthStart && d.CreatedAt.Value.Date < prevMonthEnd &&
                                                 (d.Address == "EPI" || d.Address == "EPE")) || // <-- Previous month: EPI and EPE
                                                (d.CreatedAt.Value.Date >= monthStart && d.CreatedAt.Value.Date < monthEnd &&
                                                 addressList.Contains(d.Address)) // <-- Selected month: all addresses
                                            ) &&
                                            d.CreatedAt.HasValue)
                                        .OrderBy(d => d.CreatedAt).AsQueryable()
                                        .ToListAsync();
                                }
                            }

                            var dailyGroups = allDeviceData
                                .GroupBy(d => new { Day = d.CreatedAt.Value.Date, d.Address });

                            var dailyTotals = new Dictionary<(DateTime Day, string Address), double>();

                            foreach (var group in dailyGroups)
                            {
                                var first = group.FirstOrDefault();
                                var last = group.LastOrDefault();

                                if (first != null && last != null)
                                {
                                    var diff = (last.AddressVariable ?? 0) - (first.AddressVariable ?? 0);
                                    dailyTotals[(group.Key.Day, group.Key.Address)] = diff ;
                                }
                            }

                            // Previous month: EPI and EPE
                            foreach (var address in new[] { "EPI", "EPE" })
                            {
                                for (int day = 0; day < daysInPrevMonth; day++)
                                {
                                    var currentDate = prevMonthStart.AddDays(day);
                                    result.Add(new ConsumptionDetailsDTO
                                    {
                                        SelectedDate = currentDate,
                                        SelectedUnit = selectedUnit.Value,
                                        SelectedDevice = selectedDevices,
                                        SelectedRange = selectedRange,
                                        Address = address,
                                        AddressVariable = dailyTotals.TryGetValue((currentDate, address), out var val) ? val : 0.0
                                    });
                                }
                            }

                            // Selected month: all selected addresses
                            for (int day = 0; day < daysInMonth; day++)
                            {
                                var currentDate = monthStart.AddDays(day);
                                foreach (var address in addressList)
                                {
                                    result.Add(new ConsumptionDetailsDTO
                                    {
                                        SelectedDate = currentDate,
                                        SelectedUnit = selectedUnit.Value,
                                        SelectedDevice = selectedDevices,
                                        SelectedRange = selectedRange,
                                        Address = address,
                                        AddressVariable = dailyTotals.TryGetValue((currentDate, address), out var val) ? val : 0.0
                                    });
                                }
                            }

                            break;
                        }

                    case "year":
                        {
                            DateTime yearStart = new DateTime(selectedDateTime.Year, 1, 1);
                            DateTime yearEnd = yearStart.AddYears(1);

                            DateTime prevYearStart = yearStart.AddYears(-1);
                            DateTime prevYearEnd = yearStart;

                            var previousYearAddresses = new[] { "EPI", "EPE" };
                            var allAddressesToFetch = addressList.Concat(previousYearAddresses).Distinct().ToList();

                            var deviceIds = await DBEMSContext.Devices
                                .Where(dev => dev.FkUnitId == selectedUnit.Value)
                                .Select(dev => dev.Id)
                                .ToListAsync();

                            if (deviceIds.Any())
                            {
                                var dataMasterIds = await DBEMSContext.DeviceDataMasters
                                    .Where(dm =>
                                        dm.FkDeviceId.HasValue &&
                                        deviceIds.Contains(dm.FkDeviceId.Value) &&
                                        dm.CreatedAt.HasValue &&
                                        dm.CreatedAt >= prevYearStart &&
                                        dm.CreatedAt < yearEnd)
                                    .Select(dm => dm.Id)
                                    .ToListAsync();

                                if (dataMasterIds.Any())
                                {
                                    allDeviceData = await DBEMSContext.DeviceDataDetails
                                        .Where(d =>
                                            d.FkDeviceDataMasterId.HasValue &&
                                            dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                            d.CreatedAt.HasValue &&
                                            d.CreatedAt >= prevYearStart &&
                                            d.CreatedAt < yearEnd &&
                                            allAddressesToFetch.Contains(d.Address))
                                        .OrderBy(d => d.CreatedAt)
                                        .ToListAsync();
                                }
                            }

                            var monthlyGroups = allDeviceData
                                .GroupBy(d => new
                                {
                                    Month = new DateTime(d.CreatedAt.Value.Year, d.CreatedAt.Value.Month, 1),
                                    d.Address
                                });

                            var monthlyTotals = new Dictionary<(DateTime Month, string Address), double>();

                            foreach (var group in monthlyGroups)
                            {
                                var first = group.FirstOrDefault();
                                var last = group.LastOrDefault();

                                if (first != null && last != null)
                                {
                                    var diff = (last.AddressVariable ?? 0) - (first.AddressVariable ?? 0);
                                    monthlyTotals[(group.Key.Month, group.Key.Address)] = diff ;
                                }
                            }

                            // Previous year: EPI and EPE
                            foreach (var address in previousYearAddresses)
                            {
                                for (int month = 1; month <= 12; month++)
                                {
                                    var prevMonth = new DateTime(selectedDateTime.Year - 1, month, 1);
                                    result.Add(new ConsumptionDetailsDTO
                                    {
                                        SelectedDate = prevMonth,
                                        SelectedUnit = selectedUnit.Value,
                                        SelectedDevice = selectedDevices,
                                        SelectedRange = selectedRange,
                                        Address = address,
                                        AddressVariable = monthlyTotals.TryGetValue((prevMonth, address), out var val) ? val : 0.0
                                    });
                                }
                            }

                            // Current year: all selected addresses
                            for (int month = 1; month <= 12; month++)
                            {
                                var currMonth = new DateTime(selectedDateTime.Year, month, 1);
                                foreach (var address in addressList)
                                {
                                    result.Add(new ConsumptionDetailsDTO
                                    {
                                        SelectedDate = currMonth,
                                        SelectedUnit = selectedUnit.Value,
                                        SelectedDevice = selectedDevices,
                                        SelectedRange = selectedRange,
                                        Address = address,
                                        AddressVariable = monthlyTotals.TryGetValue((currMonth, address), out var val) ? val : 0.0
                                    });
                                }
                            }

                            break;
                        }


                    default:
                        throw new ArgumentException("Invalid range specified");
                }
                return result;
            }
            if (selectedDevices == null)
            {
            }
            switch (selectedRange?.ToLower())
            {
                case "date":
                    {
                        var previousDate = selectedDateTime.Date.AddDays(-1); // Previous day

                        // Get dataMasterIds for current and previous date
                        var dataMasterIds = await DBEMSContext.DeviceDataMasters
                            .Where(x => x.FkDeviceId == selectedDevices &&
                                        x.CreatedAt.HasValue &&
                                        (x.CreatedAt.Value.Date == selectedDateTime.Date ||
                                         x.CreatedAt.Value.Date == previousDate))
                            .Select(x => x.Id)
                            .ToListAsync();

                        // Get all data details: current date (all addresses), previous date (EPI and EPE only)
                        allDeviceData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                d.CreatedAt.HasValue &&
                                (
                                    (d.CreatedAt.Value.Date == previousDate && (d.Address == "EPI" || d.Address == "EPE")) ||
                                    (d.CreatedAt.Value.Date == selectedDateTime.Date && addressList.Contains(d.Address))
                                )
                            )
                            .OrderBy(d => d.CreatedAt)
                            .ToListAsync();

                        // Group by date, hour, and address
                        var hourlyGroups = allDeviceData
                            .GroupBy(d => new { Date = d.CreatedAt.Value.Date, Hour = d.CreatedAt.Value.Hour, d.Address });

                        var hourlyTotals = new Dictionary<(DateTime Date, int Hour, string Address), double>();

                        foreach (var group in hourlyGroups)
                        {
                            var first = group.FirstOrDefault();
                            var last = group.LastOrDefault();

                            if (first != null && last != null)
                            {
                                var diff = (last.AddressVariable ?? 0) - (first.AddressVariable ?? 0);
                                var key = (group.Key.Date, group.Key.Hour, group.Key.Address);

                                hourlyTotals[key] = diff ;
                            }
                        }

                        // Previous date: add both EPI and EPE
                        foreach (var address in new[] { "EPI", "EPE" })
                        {
                            for (int hour = 0; hour < 24; hour++)
                            {
                                result.Add(new ConsumptionDetailsDTO
                                {
                                    SelectedDate = previousDate.AddHours(hour),
                                    SelectedUnit = selectedUnit ?? 0,
                                    SelectedDevice = selectedDevices.Value,
                                    SelectedRange = "date",
                                    Address = address,
                                    AddressVariable = hourlyTotals.TryGetValue((previousDate, hour, address), out var val) ? val : 0.0
                                });
                            }
                        }

                        // Current date: add all selected addresses
                        foreach (var address in addressList)
                        {
                            for (int hour = 0; hour < 24; hour++)
                            {
                                result.Add(new ConsumptionDetailsDTO
                                {
                                    SelectedDate = selectedDateTime.Date.AddHours(hour),
                                    SelectedUnit = selectedUnit ?? 0,
                                    SelectedDevice = selectedDevices.Value,
                                    SelectedRange = "date",
                                    Address = address,
                                    AddressVariable = hourlyTotals.TryGetValue((selectedDateTime.Date, hour, address), out var val) ? val : 0.0
                                });
                            }
                        }

                        break;
                    }

                case "week":
                    {
                        var startOfWeek = selectedDateTime.Date;
                        var endOfWeek = startOfWeek.AddDays(7);

                        var prevWeekStart = startOfWeek.AddDays(-7);
                        var prevWeekEnd = startOfWeek;

                        var dataMasterIds = await DBEMSContext.DeviceDataMasters
                            .Where(x => x.FkDeviceId == selectedDevices &&
                                        x.CreatedAt.HasValue &&
                                        ((x.CreatedAt.Value.Date >= startOfWeek && x.CreatedAt.Value.Date < endOfWeek) ||
                                         (x.CreatedAt.Value.Date >= prevWeekStart && x.CreatedAt.Value.Date < prevWeekEnd)))
                            .Select(x => x.Id)
                            .ToListAsync();

                        allDeviceData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                d.CreatedAt.HasValue &&
                                (
                                    // Current week: all selected addresses
                                    (d.CreatedAt.Value.Date >= startOfWeek && d.CreatedAt.Value.Date < endOfWeek && addressList.Contains(d.Address)) ||
                                    // Previous week: only EPI and EPE
                                    (d.CreatedAt.Value.Date >= prevWeekStart && d.CreatedAt.Value.Date < prevWeekEnd && (d.Address == "EPI" || d.Address == "EPE"))
                                ))
                            .OrderBy(d => d.CreatedAt)
                            .ToListAsync();

                        var groupedData = allDeviceData.GroupBy(d => new { Date = d.CreatedAt.Value.Date, d.Address });

                        var dailyTotals = new Dictionary<(DateTime Date, string Address), double>();

                        foreach (var group in groupedData)
                        {
                            var first = group.FirstOrDefault();
                            var last = group.LastOrDefault();

                            if (first != null && last != null)
                            {
                                dailyTotals[(group.Key.Date, group.Key.Address)] =
                                    ((last.AddressVariable ?? 0) - (first.AddressVariable ?? 0)) ;
                            }
                        }

                        // Previous week: add both EPI and EPE
                        foreach (var day in Enumerable.Range(0, 7))
                        {
                            var currentDate = prevWeekStart.AddDays(day);
                            foreach (var address in new[] { "EPI", "EPE" })
                            {
                                result.Add(new ConsumptionDetailsDTO
                                {
                                    SelectedDate = currentDate,
                                    SelectedUnit = selectedUnit ?? 0,
                                    SelectedDevice = selectedDevices.Value,
                                    SelectedRange = "week",
                                    Address = address,
                                    AddressVariable = dailyTotals.TryGetValue((currentDate, address), out var val) ? val : 0.0
                                });
                            }
                        }

                        // Current week: all selected addresses
                        foreach (var day in Enumerable.Range(0, 7))
                        {
                            var currentDate = startOfWeek.AddDays(day);
                            foreach (var address in addressList)
                            {
                                result.Add(new ConsumptionDetailsDTO
                                {
                                    SelectedDate = currentDate,
                                    SelectedUnit = selectedUnit ?? 0,
                                    SelectedDevice = selectedDevices.Value,
                                    SelectedRange = "week",
                                    Address = address,
                                    AddressVariable = dailyTotals.TryGetValue((currentDate, address), out var val) ? val : 0.0
                                });
                            }
                        }

                        break;
                    }

                case "month":
                    {
                        var monthStart = new DateTime(selectedDateTime.Year, selectedDateTime.Month, 1);
                        var monthEnd = monthStart.AddMonths(1);
                        var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);

                        var prevMonthStart = monthStart.AddMonths(-1);
                        var prevMonthEnd = monthStart;
                        var daysInPrevMonth = DateTime.DaysInMonth(prevMonthStart.Year, prevMonthStart.Month);

                        var dataMasterIds = await DBEMSContext.DeviceDataMasters
                            .Where(x => x.FkDeviceId == selectedDevices &&
                                        x.CreatedAt.HasValue &&
                                        ((x.CreatedAt.Value.Date >= prevMonthStart && x.CreatedAt.Value.Date < prevMonthEnd) ||
                                         (x.CreatedAt.Value.Date >= monthStart && x.CreatedAt.Value.Date < monthEnd)))
                            .Select(x => x.Id).AsQueryable()
                            .ToListAsync();

                        allDeviceData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                d.CreatedAt.HasValue &&
                                (
                                    // Previous month: EPI and EPE only
                                    (d.CreatedAt.Value.Date >= prevMonthStart && d.CreatedAt.Value.Date < prevMonthEnd &&
                                        (d.Address == "EPI" || d.Address == "EPE")) ||
                                    // Current month: all selected addresses
                                    (d.CreatedAt.Value.Date >= monthStart && d.CreatedAt.Value.Date < monthEnd &&
                                        addressList.Contains(d.Address))
                                ))
                            .OrderBy(d => d.CreatedAt).AsQueryable()
                            .ToListAsync();

                        var groupedData = allDeviceData
                            .GroupBy(d => new { Date = d.CreatedAt.Value.Date, d.Address });

                        var dailyTotals = new Dictionary<(DateTime Date, string Address), double>();

                        foreach (var group in groupedData)
                        {
                            var first = group.FirstOrDefault();
                            var last = group.LastOrDefault();

                            if (first != null && last != null)
                            {
                                dailyTotals[(group.Key.Date, group.Key.Address)] =
                                    ((last.AddressVariable ?? 0) - (first.AddressVariable ?? 0)) ;
                            }
                        }

                        // Previous month - EPI and EPE only
                        for (int day = 0; day < daysInPrevMonth; day++)
                        {
                            var currentDate = prevMonthStart.AddDays(day);
                            foreach (var address in new[] { "EPI", "EPE" })
                            {
                                result.Add(new ConsumptionDetailsDTO
                                {
                                    SelectedDate = currentDate,
                                    SelectedUnit = selectedUnit ?? 0,
                                    SelectedDevice = selectedDevices.Value,
                                    SelectedRange = "month",
                                    Address = address,
                                    AddressVariable = dailyTotals.TryGetValue((currentDate, address), out var val) ? val : 0.0
                                });
                            }
                        }

                        // Selected month - all selected addresses
                        for (int day = 0; day < daysInMonth; day++)
                        {
                            var currentDate = monthStart.AddDays(day);
                            foreach (var address in addressList)
                            {
                                result.Add(new ConsumptionDetailsDTO
                                {
                                    SelectedDate = currentDate,
                                    SelectedUnit = selectedUnit ?? 0,
                                    SelectedDevice = selectedDevices.Value,
                                    SelectedRange = "month",
                                    Address = address,
                                    AddressVariable = dailyTotals.TryGetValue((currentDate, address), out var val) ? val : 0.0
                                });
                            }
                        }

                        break;
                    }

                case "year":
                    {
                        var startOfYear = new DateTime(selectedDateTime.Year, 1, 1);
                        var endOfYear = startOfYear.AddYears(1);

                        var prevYearStart = startOfYear.AddYears(-1);
                        var prevYearEnd = startOfYear;

                        var addressesToFetch = addressList.Concat(new[] { "EPI", "EPE" }).Distinct().ToList();

                        // Fetch DeviceDataMaster IDs within both years
                        var dataMasterIds = await DBEMSContext.DeviceDataMasters
                            .Where(x => x.FkDeviceId == selectedDevices &&
                                        x.CreatedAt >= prevYearStart &&
                                        x.CreatedAt < endOfYear)
                            .Select(x => x.Id)
                            .ToListAsync();

                        // Fetch DeviceDataDetails
                        allDeviceData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                                d.CreatedAt >= prevYearStart &&
                                d.CreatedAt < endOfYear &&
                                addressesToFetch.Contains(d.Address))
                            .OrderBy(d => d.CreatedAt)
                            .ToListAsync();

                        var groupedData = allDeviceData
                            .GroupBy(d => new
                            {
                                Month = new DateTime(d.CreatedAt.Value.Year, d.CreatedAt.Value.Month, 1),
                                d.Address
                            });

                        var monthlyTotals = groupedData
                            .Where(g => g.FirstOrDefault() != g.LastOrDefault())
                            .ToDictionary(
                                g => (g.Key.Month, g.Key.Address),
                                g => ((g.Last().AddressVariable ?? 0) - (g.First().AddressVariable ?? 0)) 
                            );

                        // Previous year data (EPI, EPE)
                        for (int month = 1; month <= 12; month++)
                        {
                            var prevMonth = new DateTime(selectedDateTime.Year - 1, month, 1);
                            foreach (var address in new[] { "EPI", "EPE" })
                            {
                                result.Add(new ConsumptionDetailsDTO
                                {
                                    SelectedDate = prevMonth,
                                    SelectedUnit = selectedUnit ?? 0,
                                    SelectedDevice = selectedDevices.Value,
                                    SelectedRange = "year",
                                    Address = address,
                                    AddressVariable = monthlyTotals.TryGetValue((prevMonth, address), out var val) ? val : 0.0
                                });
                            }
                        }

                        // Current year data (all selected addresses)
                        for (int month = 1; month <= 12; month++)
                        {
                            var currMonth = new DateTime(selectedDateTime.Year, month, 1);
                            foreach (var address in addressList)
                            {
                                result.Add(new ConsumptionDetailsDTO
                                {
                                    SelectedDate = currMonth,
                                    SelectedUnit = selectedUnit ?? 0,
                                    SelectedDevice = selectedDevices.Value,
                                    SelectedRange = "year",
                                    Address = address,
                                    AddressVariable = monthlyTotals.TryGetValue((currMonth, address), out var val) ? val : 0.0
                                });
                            }
                        }

                        break;
                    }


                default:
                    throw new ArgumentException("Invalid range specified");
            }
            return result;
        }
        public async Task<List<UnitDTO>> getUnits()
        {
            var data = await DBEMSContext.Units.
                Where(x => x.IsDeleted == false)
                .Select(x => new UnitDTO
                {
                    Id = x.Id,
                    Name = x.Name
                }).AsQueryable()
                .ToListAsync();

            return data;
        }

        public async Task<List<DeviceDTO>> getDevices()
        {
            var data = await DBEMSContext.Devices.
                Where(x => x.IsDeleted == false)
                .Select(x => new DeviceDTO
                {
                    Id = x.Id,
                    Name = x.Name
                }).AsQueryable()
                .ToListAsync();

            return data;
        }



        public async Task<List<PowerLoadDTO>> PowerConsumption()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddDays(-6); // Last 7 days
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id).AsQueryable()
                    .ToListAsync();

                if (deviceIds != null && deviceIds.Count > 0)
                {
                    // Temporary dictionary to accumulate daily totals
                    var dailyTotals = new Dictionary<DateTime, double>();

                    foreach (var deviceId in deviceIds)
                    {
                        var dataMasterIds = await DBEMSContext.DeviceDataMasters
                            .Where(dm => dm.FkDeviceId == deviceId &&
                                         dm.CreatedAt >= startTime && dm.CreatedAt <= endTime)
                            .Select(dm => new { dm.Id }).AsQueryable()
                            .ToListAsync();

                        if (dataMasterIds.Count == 0) continue;

                        var allDeviceData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Select(x => x.Id).Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "EPI" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime).AsQueryable()
                            .OrderBy(d => d.CreatedAt)
                            .ToListAsync();

                        var dailyGroups = allDeviceData
                            .GroupBy(d => d.CreatedAt.Value.Date)
                            .ToList();

                        foreach (var group in dailyGroups)
                        {
                            var first = group.FirstOrDefault();
                            var last = group.LastOrDefault();

                            if (first != null && last != null)
                            {
                                var devicediff = ((last.AddressVariable ?? 0) - (first.AddressVariable ?? 0)) ;
                                // apply multiplier here

                                if (dailyTotals.ContainsKey(group.Key))
                                {
                                    dailyTotals[group.Key] += devicediff;
                                }
                                else
                                {
                                    dailyTotals[group.Key] = devicediff;
                                }
                            }
                        }
                    }

                    // Now create the final result from the dailyTotals dictionary
                    foreach (var kvp in dailyTotals.OrderBy(x => x.Key))
                    {
                        result.Add(new PowerLoadDTO
                        {
                            UnitName = unit.Name,
                            AddressVariable = kvp.Value,
                            CreatedAt = kvp.Key
                        });
                    }
                }
            }

            return result;
        }




        public async Task<List<PowerLoadDTO>> PowerConsumptionv1()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddDays(-6); // Last 7 days
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(1).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id).AsQueryable()
                    .ToListAsync();

                if (deviceIds != null && deviceIds.Count > 0)
                {
                    // Temporary dictionary to accumulate daily totals
                    var dailyTotals = new Dictionary<DateTime, double>();

                    foreach (var deviceId in deviceIds)
                    {
                        var dataMasterIds = await DBEMSContext.DeviceDataMasters
                            .Where(dm => dm.FkDeviceId == deviceId &&
                                         dm.CreatedAt >= startTime && dm.CreatedAt <= endTime)
                            .Select(dm => new { dm.Id }).AsQueryable()
                            .ToListAsync();

                        if (dataMasterIds.Count == 0) continue;

                        var allDeviceData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Select(x => x.Id).Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "EPI" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime).AsQueryable()
                            .OrderBy(d => d.CreatedAt)
                            .ToListAsync();

                        var dailyGroups = allDeviceData
                            .GroupBy(d => d.CreatedAt.Value.Date).AsQueryable()
                            .ToList();

                        foreach (var group in dailyGroups)
                        {
                            var first = group.FirstOrDefault();
                            var last = group.LastOrDefault();

                            if (first != null && last != null)
                            {
                                var devicediff = ((last.AddressVariable ?? 0) - (first.AddressVariable ?? 0)) ;
                                // apply multiplier here

                                if (dailyTotals.ContainsKey(group.Key))
                                {
                                    dailyTotals[group.Key] += devicediff;
                                }
                                else
                                {
                                    dailyTotals[group.Key] = devicediff;
                                }
                            }
                        }
                    }

                    // Now create the final result from the dailyTotals dictionary
                    foreach (var kvp in dailyTotals.OrderBy(x => x.Key))
                    {
                        result.Add(new PowerLoadDTO
                        {
                            UnitName = unit.Name,
                            AddressVariable = kvp.Value,
                            CreatedAt = kvp.Key
                        });
                    }
                }
            }

            return result;
        }



        public async Task<List<PowerLoadDTO>> PowerConsumptionv2()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddDays(-6); // Last 7 days
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(2).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id).AsQueryable()
                    .ToListAsync();

                if (deviceIds != null && deviceIds.Count > 0)
                {
                    // Temporary dictionary to accumulate daily totals
                    var dailyTotals = new Dictionary<DateTime, double>();

                    foreach (var deviceId in deviceIds)
                    {
                        var dataMasterIds = await DBEMSContext.DeviceDataMasters
                            .Where(dm => dm.FkDeviceId == deviceId &&
                                         dm.CreatedAt >= startTime && dm.CreatedAt <= endTime)
                            .Select(dm => new { dm.Id }).AsQueryable()
                            .ToListAsync();

                        if (dataMasterIds.Count == 0) continue;

                        var allDeviceData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Select(x => x.Id).Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "EPI" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime).AsQueryable()
                            .OrderBy(d => d.CreatedAt)
                            .ToListAsync();

                        var dailyGroups = allDeviceData
                            .GroupBy(d => d.CreatedAt.Value.Date)
                            .ToList();

                        foreach (var group in dailyGroups)
                        {
                            var first = group.FirstOrDefault();
                            var last = group.LastOrDefault();

                            if (first != null && last != null)
                            {
                                var devicediff = ((last.AddressVariable ?? 0) - (first.AddressVariable ?? 0)) ;
                                // apply multiplier here

                                if (dailyTotals.ContainsKey(group.Key))
                                {
                                    dailyTotals[group.Key] += devicediff;
                                }
                                else
                                {
                                    dailyTotals[group.Key] = devicediff;
                                }
                            }
                        }
                    }

                    // Now create the final result from the dailyTotals dictionary
                    foreach (var kvp in dailyTotals.OrderBy(x => x.Key))
                    {
                        result.Add(new PowerLoadDTO
                        {
                            UnitName = unit.Name,
                            AddressVariable = kvp.Value,
                            CreatedAt = kvp.Key
                        });
                    }
                }
            }

            return result;
        }

        //public async Task<List<PowerLoadDTO>> AllUnitsConsumption()
        //{
        //    var endTime = DateTime.Now;
        //    var startTime = endTime.Date.AddDays(-6); // Last 7 days including today
        //    var result = new List<PowerLoadDTO>();

        //    var units = await DBEMSContext.Units.ToListAsync();

        //    foreach (var unit in units)
        //    {
        //        var deviceIds = await DBEMSContext.Devices
        //            .Where(dev => dev.FkUnitId == unit.Id)
        //            .Select(dev => dev.Id)
        //            .ToListAsync();

        //        if (!deviceIds.Any())
        //            continue;

        //        var dataMasterIds = await DBEMSContext.DeviceDataMasters
        //            .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
        //            .Select(dm => dm.Id)
        //            .ToListAsync();

        //        if (!dataMasterIds.Any())
        //            continue;

        //        var rawData = await DBEMSContext.DeviceDataDetails
        //            .Where(d =>
        //                d.FkDeviceDataMasterId.HasValue &&
        //                dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
        //                d.Address == "EPI" &&
        //                d.CreatedAt >= startTime &&
        //                d.CreatedAt <= endTime)
        //            .OrderBy(d => d.CreatedAt)
        //            .ToListAsync();

        //        var dailyGroups = rawData
        //            .GroupBy(d => d.CreatedAt.Value.Date)
        //            .ToList();

        //        foreach (var group in dailyGroups)
        //        {
        //            var first = group.OrderBy(d => d.CreatedAt).FirstOrDefault();
        //            var last = group.OrderByDescending(d => d.CreatedAt).FirstOrDefault();

        //            if (first != null && last != null && last.AddressVariable >= first.AddressVariable)
        //            {
        //                result.Add(new PowerLoadDTO
        //                {
        //                    UnitName = unit.Name,
        //                    AddressVariable = (last.AddressVariable - first.AddressVariable) * 0.06,
        //                    CreatedAt = group.Key
        //                });
        //            }
        //        }
        //    }

        //    return result;
        //}

        public async Task<List<AlertCenterDataDTO>> GetAllAlertCenterData()
        {
            var result = await (
          from alert in DBEMSContext.AlertCenterData
          join device in DBEMSContext.Devices on alert.FkDeviceId equals device.Id into deviceJoin
          from device in deviceJoin.DefaultIfEmpty()
          join unit in DBEMSContext.Units on alert.FkUnitId equals unit.Id into unitJoin
          from unit in unitJoin.DefaultIfEmpty()
          where alert.IsDeleted == false
          orderby alert.CreatedAt descending
          select new AlertCenterDataDTO
          {
              Id = alert.Id,
              FkUnitId = unit.Id,
              FkDeviceId = device.Id,
              DeviceName = device != null ? device.Name : null,
              UnitName = unit != null ? unit.Name : null,
              Min = alert.Min,
              Max = alert.Max,
              Address = alert.Address,
              AlertLevel = alert.AlertLevel,
              CreatedAt = alert.CreatedAt,
              IsDeleted = alert.IsDeleted
          }).ToListAsync();

            return result;
        }

        public async Task<List<AlertCenterDTO>> GetResolveCenter()
        {
            var result = await (
                from alert in DBEMSContext.AlertCenter
                join device in DBEMSContext.Devices on alert.FkDeviceId equals device.Id
                join unit in DBEMSContext.Units on alert.FkUnitId equals unit.Id
                orderby alert.createdAt descending
                select new AlertCenterDTO
                {
                    id = alert.id,
                    DeviceName = device.Name,
                    UnitName = unit.Name,
                    AlertLevel = alert.AlertLevel,
                    Event = alert.Event,
                    CreatedAt = alert.createdAt,
                    isDeleted = alert.isDeleted
                }).AsQueryable().ToListAsync();

            return result;
        }



        public async void getdata()
        {
            var units = await DBEMSContext.Units.ToListAsync();

            foreach (var unit in units)
            {
                if (unit != null)
                {

                }
            }
        }

        public async Task<List<PowerLoadDTO>> GetPowerLoadu3()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-11);
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(2).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id).AsQueryable()
                    .ToListAsync();

                if (deviceIds.Any())
                {
                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
                        .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                        .Select(dm => new { dm.Id, dm.FkDeviceId }).AsQueryable()
                        .ToListAsync();

                    if (dataMasterIds.Any())
                    {
                        var rawData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Select(dm => dm.Id).Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "P" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime).AsQueryable()
                            .ToListAsync();

                        var minutes = Enumerable.Range(0, (int)(endTime - startTime).TotalMinutes + 1)
                            .Select(i => startTime.AddMinutes(i)).AsQueryable()
                            .ToList();

                        foreach (var minute in minutes)
                        {
                            var minuteStart = new DateTime(minute.Year, minute.Month, minute.Day, minute.Hour, minute.Minute, 0);
                            var minuteEnd = minuteStart.AddMinutes(1);

                            double? sumPerMinute = 0;

                            foreach (var deviceId in deviceIds)
                            {
                                var masterIdsForDevice = dataMasterIds
                                    .Where(dm => dm.FkDeviceId == deviceId)
                                    .Select(dm => dm.Id).AsQueryable()
                                    .ToList();

                                var deviceDataInMinute = rawData
                                    .Where(d =>
                                        masterIdsForDevice.Contains(d.FkDeviceDataMasterId.Value) &&
                                        d.CreatedAt >= minuteStart &&
                                        d.CreatedAt < minuteEnd).AsQueryable()
                                    .OrderByDescending(d => d.CreatedAt)
                                    .FirstOrDefault(); // Take latest value for this device in that minute

                                if (deviceDataInMinute != null)
                                {
                                    sumPerMinute += deviceDataInMinute.AddressVariable;
                                }
                            }

                            result.Add(new PowerLoadDTO
                            {
                                UnitName = unit.Name,
                                AddressVariable = sumPerMinute , // like before
                                CreatedAt = minuteStart
                            });
                        }
                    }
                }
            }

            return result;

        }



        public async Task<List<PowerLoadDTO>> GetPowerLoadu2()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-11);
            var result = new List<PowerLoadDTO>();

            var unit = await DBEMSContext.Units.OrderBy(u => u.Id).Skip(1).FirstOrDefaultAsync();

            if (unit != null)
            {
                var deviceIds = await DBEMSContext.Devices
                    .Where(dev => dev.FkUnitId == unit.Id)
                    .Select(dev => dev.Id).AsQueryable()
                    .ToListAsync();

                if (deviceIds.Any())
                {
                    var dataMasterIds = await DBEMSContext.DeviceDataMasters
                        .Where(dm => dm.FkDeviceId.HasValue && deviceIds.Contains(dm.FkDeviceId.Value))
                        .Select(dm => new { dm.Id, dm.FkDeviceId }).AsQueryable()
                        .ToListAsync();

                    if (dataMasterIds.Any())
                    {
                        var rawData = await DBEMSContext.DeviceDataDetails
                            .Where(d =>
                                d.FkDeviceDataMasterId.HasValue &&
                                dataMasterIds.Select(dm => dm.Id).Contains(d.FkDeviceDataMasterId.Value) &&
                                d.Address == "P" &&
                                d.CreatedAt >= startTime &&
                                d.CreatedAt <= endTime)
                            .ToListAsync();

                        var minutes = Enumerable.Range(0, (int)(endTime - startTime).TotalMinutes + 1)
                            .Select(i => startTime.AddMinutes(i))
                            .ToList();

                        foreach (var minute in minutes)
                        {
                            var minuteStart = new DateTime(minute.Year, minute.Month, minute.Day, minute.Hour, minute.Minute, 0);
                            var minuteEnd = minuteStart.AddMinutes(1);

                            double? sumPerMinute = 0;

                            foreach (var deviceId in deviceIds)
                            {
                                var masterIdsForDevice = dataMasterIds
                                    .Where(dm => dm.FkDeviceId == deviceId).AsQueryable()
                                    .Select(dm => dm.Id)
                                    .ToList();

                                var deviceDataInMinute = rawData
                                    .Where(d =>
                                        masterIdsForDevice.Contains(d.FkDeviceDataMasterId.Value) &&
                                        d.CreatedAt >= minuteStart &&
                                        d.CreatedAt < minuteEnd).AsQueryable()
                                    .OrderByDescending(d => d.CreatedAt)
                                    .FirstOrDefault(); // Take latest value for this device in that minute

                                if (deviceDataInMinute != null)
                                {
                                    sumPerMinute += deviceDataInMinute.AddressVariable;
                                }
                            }

                            result.Add(new PowerLoadDTO
                            {
                                UnitName = unit.Name,
                                AddressVariable = sumPerMinute , // like before
                                CreatedAt = minuteStart
                            });
                        }
                    }
                }
            }

            return result;
        }


        public async Task<string> GetAlert()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-1);

            // Fetch all non-deleted alert rules
            var alertRules = await DBEMSContext.AlertCenterData
                .Where(a => !a.IsDeleted).AsQueryable()
                .ToListAsync();

            // Fetch recent raw data
            var rawData = await DBEMSContext.DeviceDataDetails
                .Where(d => d.CreatedAt >= startTime && d.CreatedAt <= endTime)
                .Select(d => new
                {
                    d.CreatedAt,
                    d.Address,
                    Value = d.AddressVariable ,
                    FkUnitId = d.DeviceDataMaster.Device.FkUnitId,
                    DeviceId = d.DeviceDataMaster.FkDeviceId
                }).AsQueryable()
                .ToListAsync();

            foreach (var rule in alertRules)
            {
                var matches = rawData
                    .Where(d =>
                        d.FkUnitId == rule.FkUnitId &&
                        d.DeviceId == rule.FkDeviceId &&
                        d.Address == rule.Address).AsQueryable()
                    .ToList();

                foreach (var data in matches)
                {
                    string eventName = null;
                    bool isAlert = false;

                    if (data.Address == "Ua" || data.Address == "Ub" || data.Address == "Uc")
                    {
                        if (data.Value < rule.Min || data.Value > rule.Max)
                        {
                            isAlert = true;
                            eventName = "Voltage Threshold";
                        }
                    }
                    else if (data.Address == "P")
                    {
                        if (data.Value > rule.Max && data.Value > 10)
                        {
                            isAlert = true;
                            eventName = "Power Surge";
                        }
                    }

                    if (isAlert)
                    {
                        var alert = new AlertCenter
                        {
                            FkDeviceId = rule.FkDeviceId,
                            FkUnitId = rule.FkUnitId,
                            AlertLevel = rule.AlertLevel,
                            Event = eventName,
                            createdAt = DateTime.Now
                        };

                        DBEMSContext.AlertCenter.Add(alert);
                        break; // Only one alert per rule per cycle
                    }
                }
            }

            await DBEMSContext.SaveChangesAsync();
            return "Data saved successfully";
        }





        public async Task<List<DeviceDTO>> GetDeviceStatus()
        {

            var devices = await DBEMSContext.Devices.Where(x => x.IsDeleted == false).ToListAsync();

            return devices.ToJson().FromJson<List<DeviceDTO>>();
        }
        public async Task<List<CheckingDeviceDTO>> checkingDevice()
        {
            DateTime endTime = DateTime.Now;
            DateTime startTime = endTime.AddSeconds(-90);

            // Get all devices with their IDs and Names
            var allDevices = await DBEMSContext.Devices
     .Where(d => d.IsDeleted != true)
     .Select(d => new { d.Id, d.Name, d.IsDeleted })
     .ToListAsync();


            // Get IDs of devices that have value > 0 for specified addresses in the last 90 seconds
            var activeDeviceIds = await DBEMSContext.DeviceDataDetails
                .Where(d =>
                    d.FkDeviceDataMasterId.HasValue &&
                    d.DeviceDataMaster != null &&
                    d.CreatedAt.HasValue &&
                    (d.Address == "Ua" || d.Address == "Ub" || d.Address == "Uc" ||
                     d.Address == "Uab" || d.Address == "Uac" || d.Address == "Ubc") &&
                    d.AddressVariable.HasValue && d.AddressVariable > 0 &&
                    d.CreatedAt.Value >= startTime &&
                    d.CreatedAt.Value <= endTime)
                .Select(d => d.DeviceDataMaster.FkDeviceId)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToListAsync();

            // Create DTO list with status set based on whether the device ID is active
            var result = allDevices.Select(d => new CheckingDeviceDTO
            {
                FkDeviceId = d.Id,
                DeviceName = d.Name,
                Status = activeDeviceIds.Contains(d.Id) ? "Online" : "Offline"
            }).ToList();

            return result;
        }






        public async Task<List<kWDTO>> GetkW()
        {
            // Preload all units
            var units = await DBEMSContext.Units.Where(x => x.IsDeleted == false).ToListAsync();

            // Get all devices and link them to their units
            var devices = await DBEMSContext.Devices
                .Select(dev => new { dev.Id, dev.FkUnitId })
                .ToListAsync();

            var deviceIds = devices.Select(d => d.Id).ToList();

            // Get the latest DeviceDataMaster per device
            var latestDeviceDataMasters = await DBEMSContext.DeviceDataMasters
                .Where(dm => deviceIds.Contains(dm.FkDeviceId.Value))
                .GroupBy(dm => dm.FkDeviceId)
                .Select(g => g.OrderByDescending(dm => dm.CreatedAt).FirstOrDefault())
                .ToListAsync();

            var latestDDMIds = latestDeviceDataMasters.Select(dm => dm.Id).ToList();

            // Get all "P" address readings from latest masters
            var deviceDataDetails = await DBEMSContext.DeviceDataDetails
                .Where(d => latestDDMIds.Contains(d.FkDeviceDataMasterId.Value) && d.Address == "P")
                .Select(d => new { d.FkDeviceDataMasterId, d.AddressVariable })
                .ToListAsync();

            // Join all data in memory
            var result = new List<kWDTO>();

            foreach (var unit in units)
            {
                // All devices in this unit
                var unitDeviceIds = devices
                    .Where(d => d.FkUnitId == unit.Id)
                    .Select(d => d.Id)
                    .ToList();

                if (!unitDeviceIds.Any())
                {
                    result.Add(new kWDTO { UnitName = unit.Name, AddressVariable = 0 });
                    continue;
                }

                // Get DDMs linked to this unit
                var unitDDMs = latestDeviceDataMasters
                    .Where(dm => unitDeviceIds.Contains(dm.FkDeviceId.Value))
                    .Select(dm => dm.Id)
                    .ToList();

                if (!unitDDMs.Any())
                {
                    result.Add(new kWDTO { UnitName = unit.Name, AddressVariable = 0 });
                    continue;
                }

                // Get relevant readings
                // Get relevant readings
                var unitReadings = deviceDataDetails
                    .Where(d => unitDDMs.Contains(d.FkDeviceDataMasterId.Value))
                    .Select(d => d.AddressVariable ?? 0)
                    .ToList();

                // Count how many readings are non-zero
                var nonZeroCount = unitReadings.Count(v => v != 0);

                // If less than 2 non-zero readings, set total to 0
                var totalPowerKW = (nonZeroCount >= 2) ? unitReadings.Sum() : 0;

                result.Add(new kWDTO
                {
                    UnitName = unit.Name,
                    AddressVariable = totalPowerKW
                });

            }

            return result;
        }

        public async Task<List<AlertCenterDTO>> GetAlertsNotices()
        {
            var result = await (from alert in DBEMSContext.AlertCenter
                                where alert.isDeleted == false
                                join device in DBEMSContext.Devices
                                on alert.FkDeviceId equals device.Id
                                join unit in DBEMSContext.Units
                                on alert.FkUnitId equals unit.Id
                                select new AlertCenterDTO
                                {
                                    //DeviceId = alert.FkDeviceId,
                                    DeviceName = device.Name,
                                    //UnitId = alert.FkUnitId,
                                    UnitName = unit.Name,
                                    AlertLevel = alert.AlertLevel,
                                    Event = alert.Event,
                                    CreatedAt = alert.createdAt
                                }).AsQueryable().ToListAsync();

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
                        .Where(dev => dev.FkUnitId == unit.Id && dev.Id == deviceid)
                        .Select(dev => dev.Id).AsQueryable()
                        .ToListAsync();

                    if (deviceIds.Any())
                    {
                        var dataMasterIds = await DBEMSContext.DeviceDataMasters
                            .Where(dm => dm.FkDeviceId == deviceid && dm.CreatedAt >= startTime && dm.CreatedAt <= endTime).AsQueryable()
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
                                }).AsQueryable()
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
                        case "ubc":
                        case "uab":
                        case "uac":
                        case "pa":
                        case "qa":
                        case "q":
                        case "ia":
                        case "pfa":
                        case "pfb":
                        case "pfc":
                        case "pf":

                            // Ensure no divide by zero occurs
                            if (lastValue != 0)
                            {
                                calculatedValue = lastValue ; // Divide by 20 as per original logic
                            }
                            else
                            {
                                calculatedValue = 0; // Handle case where lastValue is 0
                            }
                            break;

                        case "epi":
                        case "epe":
                        case "eqc":
                        case "eql":

                            if (lastValue != 0 && firstValue != 0)
                            {
                                calculatedValue = lastValue ; // EPI formula
                            }
                            break;

                        case "freq":
                            if (lastValue != 0 && firstValue != 0)
                            {
                                calculatedValue = lastValue ;
                            }
                            break;
                        default:
                            calculatedValue = lastValue; // Default to last value
                            break;
                    }

                    return new DeviceDataDetailDTO
                    {
                        //UnitId = last.UnitId,
                        Address = g.Key.Address,
                        AddressVariable = calculatedValue,
                        CreatedAt = g.Key.TimeBucket,
                        //DeviceId = last.DeviceId
                    };
                })
                .ToList();

            return groupedData;
        }

        public async Task<List<DeviceDataDetailsHistorical>> GetHistoricDeviceDataDetailsAsync(
     DateTime startDate, DateTime endDate, string parameter, int deviceID)
        {
            var lowerParam = parameter.ToLower();
            var result = new List<DeviceDataDetailsHistorical>();

            var dataMasterIds = await DBEMSContext.DeviceDataMasters
                .Where(dm => dm.FkDeviceId == deviceID && dm.CreatedAt >= startDate && dm.CreatedAt <= endDate)
                .Select(dm => dm.Id)
                .ToListAsync();

            if (dataMasterIds.Any())
            {
                var data = await DBEMSContext.DeviceDataDetails
                    .Where(d =>
                        d.FkDeviceDataMasterId.HasValue &&
                        dataMasterIds.Contains(d.FkDeviceDataMasterId.Value) &&
                        d.Address.ToLower() == lowerParam &&
                        d.CreatedAt >= startDate &&
                        d.CreatedAt <= endDate)
                    .Select(d => new DeviceDataDetailsHistorical
                    {
                        Address = d.Address,
                        AddressVariable = d.AddressVariable,
                        CreatedAt = d.CreatedAt,
                    }).AsQueryable()
                    .ToListAsync();

                result.AddRange(data);
            }

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
                        0
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
                    double lastValue = last.AddressVariable ?? 0;
                    double firstValue = first.AddressVariable ?? 0;

                    string addr = g.Key.Address?.ToLower() ?? "";

                    switch (addr)
                    {
                        case "ua":
                        case "ub":
                        case "uc":
                        case "p":
                        case "ubc":
                        case "uab":
                        case "uac":
                        case "pa":
                        case "qa":
                        case "q":
                        case "ia":
                        case "pfa":
                        case "pfb":
                        case "pfc":
                        case "pf":
                            if (lastValue != 0)
                            {
                                calculatedValue = lastValue ;
                            }
                            else
                            {
                                calculatedValue = 0;
                            }
                            break;

                        case "epi":
                        case "epe":
                        case "eqc":
                        case "eql":
                            if (lastValue != 0 && firstValue != 0)
                            {
                                calculatedValue = lastValue ;
                            }
                            else
                            {
                                calculatedValue = 0;
                            }
                            break;

                        case "freq":
                            if (lastValue != 0)
                            {
                                calculatedValue = lastValue ;
                            }
                            else
                            {
                                calculatedValue = 0;
                            }
                            break;

                        default:
                            calculatedValue = lastValue;
                            break;
                    }

                    return new DeviceDataDetailsHistorical
                    {
                        Address = g.Key.Address,
                        AddressVariable = calculatedValue,
                        CreatedAt = g.Key.TimeBucket,
                    };
                })
                .ToList();

            return groupedData;
        }

        public async Task<List<UnitDTO>> GetUnitsDetails()
        {
            var data = await DBEMSContext.Units
                .Where(x => x.IsDeleted == false)
                .Select(x => new UnitDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    // Map other fields as needed
                }).AsQueryable()
                .ToListAsync();

            return data;
        }


        public async Task<List<DeviceDTO>> Getdevices(int unitid)
        {
            var data = await DBEMSContext.Devices
                .Where(x => x.IsDeleted == false && x.FkUnitId == unitid)
                .Select(x => new DeviceDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    // Map other fields as needed
                })
                .ToListAsync();

            return data;
        }

        public async Task<List<DeviceDataDetailDTO>> GetAddresses()
        {
            var data = await DBEMSContext.DeviceDataDetails

                .Select(x => new DeviceDataDetailDTO
                {
                    Address = x.Address
                    // Map other fields as needed
                })
                .Take(13).AsQueryable()
                .ToListAsync();

            return data;
        }

        public async Task<bool> AddAlertCenterData(AlertCenterData model)
        {
            model.CreatedAt = DateTime.Now;
            await DBEMSContext.AlertCenterData.AddAsync(model);
            await DBEMSContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAlertCenterData(AlertCenterData model, int id)
        {
            var data = DBEMSContext.AlertCenterData.FirstOrDefault(x => x.Id == id);
            if (data != null)
            {
                data.AlertLevel = model.AlertLevel;
                data.Address = model.Address;
                data.Max = model.Max;
                data.Min = model.Min;
                data.FkDeviceId = model.FkDeviceId;
                data.FkUnitId = model.FkUnitId;


                await DBEMSContext.SaveChangesAsync(); // Only save here
                return true;
            }

            return false; // No record found to update
        }

        public async Task<bool> DeleteAlertCenterData(int id)
        {
            var data = await DBEMSContext.AlertCenterData.FindAsync(id); // Slightly faster than FirstOrDefault for keys
            if (data != null)
            {
                DBEMSContext.AlertCenterData.Remove(data); // Explicitly remove from correct DbSet
                await DBEMSContext.SaveChangesAsync();
                return true;
            }

            return false; // No record found
        }



        public async Task<bool> SetResolveCenter(int id)
        {
            var data = await DBEMSContext.AlertCenter.FindAsync(id); // Efficient for primary key lookups
            if (data != null)
            {
                data.isDeleted = true;
                await DBEMSContext.SaveChangesAsync();
                return true;
            }

            return false; // No record found
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
                .Where(u => u.UnitId != 0).AsQueryable()
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
                                .Contains(d.DeviceDataMaster.FkDeviceId.Value)).AsQueryable()
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


        public async Task<List<UnitWiseAddressVariableSumDTO>> Getpowerloadtoday()
        {

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
                ).AsQueryable().ToListAsync();


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
                }).AsQueryable()
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
            ).AsQueryable().ToListAsync(); // Retrieve and group data in one step

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
            ).AsQueryable().ToListAsync();

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
                                .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault()).AsQueryable()
                                .ToList(),

                "monthly" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, Month = d.CreatedAt?.Month, d.Address })
                                 .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault()).AsQueryable()
                                 .ToList(),

                "daily" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, Month = d.CreatedAt?.Month, Day = d.CreatedAt?.Day, d.Address })
                               .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault()).AsQueryable()
                               .ToList(),

                "hourly" => data.GroupBy(d => new { Year = d.CreatedAt?.Year, Month = d.CreatedAt?.Month, Day = d.CreatedAt?.Day, Hour = d.CreatedAt?.Hour, d.Address })
                                .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault()).AsQueryable()
                                .ToList(),

                "15minutes" => data.GroupBy(d => new
                {
                    Year = d.CreatedAt?.Year,
                    Month = d.CreatedAt?.Month,
                    Day = d.CreatedAt?.Day,
                    Hour = d.CreatedAt?.Hour,
                    Quarter = d.CreatedAt?.Minute / 15,
                    d.Address
                })
                                   .Select(g => g.OrderByDescending(d => d.CreatedAt).FirstOrDefault()).AsQueryable()
                                   .ToList(),

                _ => data // 🛑 Return unmodified data if time range is invalid
            };
        }

        public Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail(IEnumerable<int> projectId, IEnumerable<int> unitId, Dictionary<int, List<int>> meterId, DateTime startDate, DateTime endDate, string timeRange)
        {
            throw new NotImplementedException();
        }

        public Task<KeyValuePair<string, string>[]> GetHistoricDeviceDataDetailsAsync(DateTime startDate, DateTime endDate, string parameter)
        {
            throw new NotImplementedException();
        }
    }

}
