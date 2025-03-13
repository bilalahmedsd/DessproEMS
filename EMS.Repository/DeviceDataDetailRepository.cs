using EMS.Core.Helpers;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Data.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<DeviceDataDetailDTO>> GetFilterDeviceDataDetails(
      int[] projectIds, int[] unitIds, int[] meterIds,
      DateTime startDate, DateTime endDate, string timeRange)
        {
            var query = from detail in DBEMSContext.DeviceDataDetails
                        join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
                        join device in DBEMSContext.Devices on master.FkDeviceId equals device.Id
                        join unit in DBEMSContext.Units on device.FkUnitId equals unit.Id
                        join project in DBEMSContext.ProjectManagements on unit.FkProjectManagement equals project.Id
                        where detail.CreatedAt >= startDate && detail.CreatedAt <= endDate
                        && (detail.Address == "EPI" || detail.Address == "EPE" || detail.Address == "EQL" || detail.Address == "EQC")
                        && projectIds.Contains(project.Id)
                        && unitIds.Contains(unit.Id)
                        && meterIds.Contains(device.Id)
                        select new { detail, master, device, unit, project };

            // Fetch Data
            var dataList = await query.ToListAsync();

            // Apply Time Range filtering if needed
            if (!string.IsNullOrEmpty(timeRange))
            {
                dataList = dataList.GroupBy(q => GetTimeGrouping(q.detail.CreatedAt, timeRange))
                                   .Select(g => g.First()) // Get one record from each group
                                   .ToList();
            }

            // Map to DTO
            var result = dataList.Select(q => new DeviceDataDetailDTO
            {
                Id = q.detail.Id,
                FkDeviceDataMasterId = q.detail.FkDeviceDataMasterId,
                Address = q.detail.Address,
                AddressVariable = q.detail.AddressVariable,
                CreatedAt = q.detail.CreatedAt,
                DeviceDataMaster = new DeviceDataMasterDTO
                {
                    Id = q.master.Id,
                    DeviceId = q.master.DeviceId,
                    FkDeviceId = q.master.FkDeviceId,
                    CreatedAt = q.master.CreatedAt,
                    Device = q.device != null ? new DeviceDTO
                    {
                        Id = q.device.Id,
                        Name = q.device.Name,
                        SerialNo = q.device.SerialNo,
                        Status = q.device.Status,
                        Unit = q.unit != null ? new UnitDTO
                        {
                            Id = q.unit.Id,
                            Name = q.unit.Name,
                            ProjectManagement = q.project != null ? new ProjectManagementDTO
                            {
                                Id = q.project.Id,
                                ProjectName = q.project.ProjectName
                            } : null
                        } : null
                    } : null
                }
            }).ToList();

            return result;
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
        }

    }
