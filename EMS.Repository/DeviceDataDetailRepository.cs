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
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

                var startOfToday = now.Date;
                var startOfYesterday = startOfToday.AddDays(-1);

                var todaySlots = new List<DateTime>();
                var slot = startOfToday;

                // ✅ Create 1-hour slots until the last full block
                while (slot.AddHours(1) <= now)
                {
                    todaySlots.Add(slot);
                    slot = slot.AddHours(1);
                }

                // ✅ Ensure the last block covers from the last full block **to NOW**
                if (slot < now)
                {
                    todaySlots.Add(slot); // Add last partial block (e.g., 12:00 to 12:45)
                }

                var yesterdaySlots = todaySlots.Select(slot => slot.AddDays(-1)).ToList();

                // 📦 Fetch all required data
                var todayRaw = await DBEMSContext.DeviceDataDetails
                    .Where(d =>
                        d.Address == "P" &&
                        d.CreatedAt >= startOfToday &&
                        d.CreatedAt <= now) // Fetch data up to current time
                    .ToListAsync();

                var yesterdayRaw = await DBEMSContext.DeviceDataDetails
                    .Where(d =>
                        d.Address == "P" &&
                        d.CreatedAt >= startOfYesterday &&
                        d.CreatedAt <= startOfYesterday + (now - startOfToday))
                    .ToListAsync();

                // 📊 Group today's data
                var todayGrouped = todaySlots.Select(slot =>
                {
                    var slotEnd = (slot.AddHours(1) > now) ? now : slot.AddHours(1); // Ensure last slot ends at current time
                    var dataInSlot = todayRaw.Where(d => d.CreatedAt >= slot && d.CreatedAt < slotEnd).ToList();

                    return new DeviceDataDetailDTO
                    {
                        CreatedAt = slot,
                        Address = "P (Today)",
                        AddressVariable = dataInSlot.Sum(d => d.AddressVariable),
                    };
                }).ToList();

                // 📊 Group yesterday's data
                var yesterdayGrouped = yesterdaySlots.Select(slot =>
                {
                    var slotEnd = (slot.AddHours(1) > now.AddDays(-1)) ? now.AddDays(-1) : slot.AddHours(1);
                    var dataInSlot = yesterdayRaw.Where(d => d.CreatedAt >= slot && d.CreatedAt < slotEnd).ToList();

                    return new DeviceDataDetailDTO
                    {
                        CreatedAt = slot,
                        Address = "P (Yesterday)",
                        AddressVariable = dataInSlot.Sum(d => d.AddressVariable),
                    };
                }).ToList();

                return todayGrouped.Concat(yesterdayGrouped).ToList();
            }





        public async Task<List<DeviceDataDetailDTO>> GetDeviceDataDetailsAsync()
        {
            try
            {

                var data = await DBEMSContext.DeviceDataDetails
                    .AsNoTracking()
                    .Where(d => d.CreatedAt >= DateTime.Now.AddSeconds(-60))
                    .OrderBy(d => d.CreatedAt)
                    .Select(d => new DeviceDataDetailDTO
                    {
                        // Map properties from DeviceDataDetail to DeviceDataDetailDTO
                        Id = d.Id,
                        Address = d.Address,
                        AddressVariable = d.AddressVariable,
                        CreatedAt = d.CreatedAt
                        // Add other properties as needed
                    })
                    .ToListAsync();
                return data.ToJson().FromJson<List<DeviceDataDetailDTO>>();
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework)
                Console.Error.WriteLine($"An error occurred: {ex.Message}");
                return new List<DeviceDataDetailDTO>(); // Return an empty list in case of error
            }
        }
        public async Task<List<DeviceDataDetailDTO>> GetHistoricDeviceDataDetailsAsync(DateTime startDate, DateTime endDate)
        {
            var data = await DBEMSContext.DeviceDataDetails
                .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                .ToListAsync();

            return data.ToJson().FromJson<List<DeviceDataDetailDTO>>();
        }


        //  public async Task<List<DeviceDataDetailDTO>> GetfilterDeviceDataDetails(
        //int? projectId, int? meterId, int? unitId, DateTime startDate, DateTime endDate, string? timeRange)
        //  {
        //      var query = from detail in DBEMSContext.DeviceDataDetails
        //                  join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
        //                  join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id
        //                  join gateway in DBEMSContext.Gateways on device.FkGatewayId equals gateway.Id
        //                  join unit in DBEMSContext.Units on gateway.FkUnitId equals unit.Id
        //                  join project in DBEMSContext.ProjectManagements on unit.FkProjectManagement equals project.Id
        //                  where detail.CreatedAt >= startDate && detail.CreatedAt <= endDate
        //                  select new { detail, master, device, gateway, unit, project };

        //      // ✅ Filters
        //      if (meterId.HasValue)
        //          query = query.Where(q => q.device.Id == meterId);
        //      if (unitId.HasValue)
        //          query = query.Where(q => q.unit.Id == unitId);
        //      if (projectId.HasValue)
        //          query = query.Where(q => q.project.Id == projectId);

        //      // ✅ Fetch Data First (To Avoid Translation Issues)
        //      var dataList =  query.AsEnumerable().ToList(); // ✅ Data pehle fetch karlo

        //      // ✅ Time Range Grouping (Client-Side After Fetch)
        //      switch (timeRange?.ToLower())
        //      {
        //          case "daily":
        //              dataList = dataList.GroupBy(q => q.detail.CreatedAt?.Date)
        //                                 .Select(g => g.First())
        //                                 .ToList();
        //              break;
        //          case "hourly":
        //              dataList = dataList.GroupBy(q => new { q.detail.CreatedAt?.Date, q.detail.CreatedAt?.Hour })
        //                                 .Select(g => g.First())
        //                                 .ToList();
        //              break;
        //          case "weekly":
        //              dataList = dataList.GroupBy(q => new
        //              {
        //                  Year = q.detail.CreatedAt?.Year,
        //                  Week = EF.Functions.DateDiffWeek(new DateTime(1900, 1, 1), q.detail.CreatedAt)
        //              })
        //              .Select(g => g.First())
        //              .ToList();
        //              break;
        //          case "monthly":
        //              dataList = dataList.GroupBy(q => new { q.detail.CreatedAt?.Year, q.detail.CreatedAt?.Month })
        //                                 .Select(g => g.First())
        //                                 .ToList();
        //              break;
        //      }

        //      // ✅ DTO Mapping
        //      var result = dataList.Select(q => new DeviceDataDetailDTO
        //      {
        //          Id = q.detail.Id,
        //          FkDeviceDataMasterId = q.detail.FkDeviceDataMasterId,
        //          Address = q.detail.Address,
        //          AddressVariable = q.detail.AddressVariable,
        //          CreatedAt = q.detail.CreatedAt,

        //          DeviceDataMaster = new DeviceDataMasterDTO
        //          {
        //              Id = q.master.Id,
        //              FkDeviceId = q.master.FkDeviceId,
        //              CreatedAt = q.master.CreatedAt,

        //              Device = new DeviceDTO
        //              {
        //                  Id = q.device.Id,
        //                  Name = q.device.Name,
        //                  SerialNo = q.device.SerialNo,
        //                  Status = q.device.Status,

        //                  Gateway = new GatewayDTO
        //                  {
        //                      Id = q.gateway.Id,
        //                      Name = q.gateway.Name,

        //                      Unit = new UnitDTO
        //                      {
        //                          Id = q.unit.Id,
        //                          Name = q.unit.Name,

        //                          ProjectManagement = new ProjectManagementDTO
        //                          {
        //                              Id = q.project.Id,
        //                              ProjectName = q.project.ProjectName
        //                          }
        //                      }
        //                  }
        //              }
        //          }
        //      }).ToList();

        //      return result;
        //  }

        //    public async Task<List<DeviceDataDetailDTO>> GetfilterDeviceDataDetails(
        //int? projectId, int? meterId, int? unitId, DateTime startDate, DateTime endDate, string? timeRange)
        //    {
        //        var query = from detail in DBEMSContext.DeviceDataDetails
        //                    join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
        //                    join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id into deviceGroup
        //                    from device in deviceGroup.DefaultIfEmpty() // ✅ Left Join for nullable values
        //                    join gateway in DBEMSContext.Gateways on device.FkGatewayId equals gateway.Id into gatewayGroup
        //                    from gateway in gatewayGroup.DefaultIfEmpty()
        //                    join unit in DBEMSContext.Units on gateway.FkUnitId equals unit.Id into unitGroup
        //                    from unit in unitGroup.DefaultIfEmpty()
        //                    join project in DBEMSContext.ProjectManagements on unit.FkProjectManagement equals project.Id into projectGroup
        //                    from project in projectGroup.DefaultIfEmpty()
        //                    where detail.CreatedAt >= startDate && detail.CreatedAt <= endDate
        //                    select new { detail, master, device, gateway, unit, project };

        //        // ✅ Filters
        //        if (meterId.HasValue)
        //            query = query.Where(q => q.device.Id == meterId);
        //        if (unitId.HasValue)
        //            query = query.Where(q => q.unit.Id == unitId);
        //        if (projectId.HasValue)
        //            query = query.Where(q => q.project.Id == projectId);

        //        // ✅ Fetch Data
        //        var dataList = await query.ToListAsync(); // ✅ No need for AsEnumerable()

        //        // ✅ Time Range Grouping (Client-Side After Fetch)
        //        if (!string.IsNullOrEmpty(timeRange))
        //        {
        //            dataList = dataList.AsEnumerable().GroupBy(q => q.detail.CreatedAt?.Date)
        //                               .Select(g => g.First())
        //                               .ToList();
        //        }

        //        // ✅ DTO Mapping
        //        var result = dataList.Select(q => new DeviceDataDetailDTO
        //        {
        //            Id = q.detail.Id,
        //            FkDeviceDataMasterId = q.detail.FkDeviceDataMasterId,
        //            Address = q.detail.Address,
        //            AddressVariable = q.detail.AddressVariable,
        //            CreatedAt = q.detail.CreatedAt,

        //            DeviceDataMaster = new DeviceDataMasterDTO
        //            {
        //                Id = q.master.Id,
        //                DeviceId = q.master != null ? q.master.DeviceId : null,
        //                FkDeviceId = q.master.FkDeviceId,
        //                CreatedAt = q.master.CreatedAt,

        //                Device = q.device != null ? new DeviceDTO
        //                {
        //                    Id = q.device.Id,
        //                    Name = q.device.Name,
        //                    SerialNo = q.device.SerialNo,
        //                    Status = q.device.Status,

        //                    Gateway = q.gateway != null ? new GatewayDTO
        //                    {
        //                        Id = q.gateway.Id,
        //                        Name = q.gateway.Name,

        //                        Unit = q.unit != null ? new UnitDTO
        //                        {
        //                            Id = q.unit.Id,
        //                            Name = q.unit.Name,

        //                            ProjectManagement = q.project != null ? new ProjectManagementDTO
        //                            {
        //                                Id = q.project.Id,
        //                                ProjectName = q.project.ProjectName
        //                            } : null
        //                        } : null
        //                    } : null
        //                } : null
        //            }
        //        }).ToList();

        //        return result;
        //    }

        // public async Task<List<DeviceDataDetailDTO>> GetfilterDeviceDataDetails(
        //int? projectId, int? meterId, int? unitId, DateTime startDate, DateTime endDate, string? timeRange)
        // {
        //     var query = from detail in DBEMSContext.DeviceDataDetails
        //                 join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
        //                 join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id into deviceGroup
        //                 from device in deviceGroup.DefaultIfEmpty()
        //                 join gateway in DBEMSContext.Gateways on device.FkGatewayId equals gateway.Id into gatewayGroup
        //                 from gateway in gatewayGroup.DefaultIfEmpty()
        //                 join unit in DBEMSContext.Units on gateway.FkUnitId equals unit.Id into unitGroup
        //                 from unit in unitGroup.DefaultIfEmpty()
        //                 join project in DBEMSContext.ProjectManagements on unit.FkProjectManagement equals project.Id into projectGroup
        //                 from project in projectGroup.DefaultIfEmpty()
        //                 where detail.CreatedAt >= startDate && detail.CreatedAt <= endDate
        //                 && (detail.Address == "EPI" || detail.Address == "EPE" || detail.Address == "EQL" || detail.Address == "EQC") // ✅ Filter Specific Data
        //                 select new { detail, master, device, gateway, unit, project };

        //     // ✅ Apply Filters
        //     if (meterId.HasValue)
        //         query = query.Where(q => q.device.Id == meterId);
        //     if (unitId.HasValue)
        //         query = query.Where(q => q.unit.Id == unitId);
        //     if (projectId.HasValue)
        //         query = query.Where(q => q.project.Id == projectId);

        //     // ✅ Fetch Data from Database (Materialization)
        //     var dataList = await query.ToListAsync();

        //     // ✅ Time-Based Filtering (AFTER Fetch)
        //     if (!string.IsNullOrEmpty(timeRange))
        //     {
        //         dataList = dataList.GroupBy(q => GetTimeGrouping(q.detail.CreatedAt, timeRange))
        //                            .Select(g => g.First()) // ✅ All records in the group
        //                            .ToList();
        //     }

        //     // ✅ DTO Mapping
        //     var result = dataList.Select(q => new DeviceDataDetailDTO
        //     {
        //         Id = q.detail.Id,
        //         FkDeviceDataMasterId = q.detail.FkDeviceDataMasterId,
        //         Address = q.detail.Address,
        //         AddressVariable = q.detail.AddressVariable,
        //         CreatedAt = q.detail.CreatedAt,

        //         DeviceDataMaster = new DeviceDataMasterDTO
        //         {
        //             Id = q.master.Id,
        //             DeviceId = q.master != null ? q.master.DeviceId : null,
        //             FkDeviceId = q.master.FkDeviceId,
        //             CreatedAt = q.master.CreatedAt,

        //             Device = q.device != null ? new DeviceDTO
        //             {
        //                 Id = q.device.Id,
        //                 Name = q.device.Name,
        //                 SerialNo = q.device.SerialNo,
        //                 Status = q.device.Status,

        //                 Gateway = q.gateway != null ? new GatewayDTO
        //                 {
        //                     Id = q.gateway.Id,
        //                     Name = q.gateway.Name,

        //                     Unit = q.unit != null ? new UnitDTO
        //                     {
        //                         Id = q.unit.Id,
        //                         Name = q.unit.Name,

        //                         ProjectManagement = q.project != null ? new ProjectManagementDTO
        //                         {
        //                             Id = q.project.Id,
        //                             ProjectName = q.project.ProjectName
        //                         } : null
        //                     } : null
        //                 } : null
        //             } : null
        //         }
        //     }).ToList();

        //     return result;
        // }
        // private DateTime GetTimeGrouping(DateTime? createdAt, string timeRange)
        // {
        //     if (!createdAt.HasValue)
        //         return DateTime.MinValue;

        //     DateTime date = createdAt.Value;

        //     return timeRange.ToLower() switch
        //     {
        //         "15minutes" => new DateTime(date.Year, date.Month, date.Day, date.Hour, (date.Minute / 15) * 15, 0),
        //         "hourly" => new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0),
        //         "daily" => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0),
        //         "weekly" => date.AddDays(-(int)date.DayOfWeek).Date,
        //         "monthly" => new DateTime(date.Year, date.Month, 1),
        //         "quarterly" => new DateTime(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1),
        //         "yearly" => new DateTime(date.Year, 1, 1),
        //         _ => date // Default: No grouping
        //     };
        // }

        public async Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataDetail(
    IEnumerable<int> projectId,
    IEnumerable<int> unitId,
    Dictionary<int, List<int>> meterId,
    DateTime startDate,
    DateTime endDate,
    string timeRange)
        {
            // ✅ Ensure proper date range
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            var validAddresses = new HashSet<string> { "EPI", "EPE", "EQL", "EQC" };

            var query = (
                from detail in DBEMSContext.DeviceDataDetails
                join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
                join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id
                join unit in DBEMSContext.Units on device.FkUnitId equals unit.Id
                join project in DBEMSContext.ProjectManagements on unit.FkProjectManagement equals project.Id
                where projectId.Contains(project.Id) &&
                      unitId.Contains(unit.Id) &&
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


    }

}
