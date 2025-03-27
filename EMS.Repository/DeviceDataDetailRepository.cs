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

            var validAddresses = new HashSet<string> { "EPI", "EPE", "EQL", "EQC" };
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
