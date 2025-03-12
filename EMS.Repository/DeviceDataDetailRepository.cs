using EMS.Core.Helpers;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //  public async Task<List<DeviceDataDetailDTO>> GetFilteredDeviceDataAsync(
        //int? projectId, int? meterId, int? unitId,
        //DateTime startDate, DateTime endDate, string timeRange)
        //  {
        //      var query = from detail in DBEMSContext.DeviceDataDetails
        //                  join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
        //                  join device in DBEMSContext.Devices on master.DeviceId equals device.Id.ToString()
        //                  join gateway in DBEMSContext.Gateways on device.FkGatewayId equals gateway.Id
        //                  join unit in DBEMSContext.Units on gateway.FkUnitId equals unit.Id
        //                  where detail.CreatedAt >= startDate && detail.CreatedAt <= endDate
        //                  select new
        //                  {
        //                      detail,
        //                      master,
        //                      device,
        //                      gateway,
        //                      unit
        //                  };

        //      // **Filters**
        //      if (meterId.HasValue)
        //          query = query.Where(q => q.device.Id == meterId); // ✅ Device ID (Meter ID)

        //      if (unitId.HasValue)
        //          query = query.Where(q => q.gateway.FkUnitId == unitId); // ✅ Unit ID

        //      if (projectId.HasValue)
        //          query = query.Where(q => q.unit.FkProjectManagement == projectId); // ✅ Project ID

        //      // **Apply Time Range Filtering**
        //      var groupedQuery = query; // Default (No Grouping)

        //      switch (timeRange.ToLower())
        //      {
        //          case "daily":
        //              groupedQuery = query.GroupBy(q => q.detail.CreatedAt.Value.Date)
        //                                  .Select(g => g.FirstOrDefault());
        //              break;

        //          case "hourly":
        //              groupedQuery = query.GroupBy(q => new { q.detail.CreatedAt.Value.Date, q.detail.CreatedAt.Value.Hour })
        //                                  .Select(g => g.FirstOrDefault());
        //              break;

        //          case "weekly":
        //              groupedQuery = query.GroupBy(q =>
        //                  new { Year = q.detail.CreatedAt.Value.Year, Week = EF.Functions.DateDiffWeek(new DateTime(1900, 1, 1), q.detail.CreatedAt) })
        //                  .Select(g => g.FirstOrDefault());
        //              break;

        //          case "monthly":
        //              groupedQuery = query.GroupBy(q => new { q.detail.CreatedAt.Value.Year, q.detail.CreatedAt.Value.Month })
        //                                  .Select(g => g.FirstOrDefault());
        //              break;
        //      }

        //      var data = await groupedQuery.Select(q => new DeviceDataDetailDTO
        //      {
        //          Id = q.detail.Id,
        //          Address = q.detail.Address,
        //          AddressVariable = q.detail.AddressVariable,
        //          CreatedAt = q.detail.CreatedAt,
        //          DeviceDataMaster = new DeviceDataMasterDTO
        //          {
        //              Id = q.master.Id,
        //              Device = new DeviceDTO
        //              {
        //                  Id = q.device.Id,
        //                  Name = q.device.Name,
        //                  Gateway = new GatewayDTO
        //                  {
        //                      Id = q.gateway.Id,
        //                      Name = q.gateway.Name,
        //                      Unit = new UnitDTO
        //                      {
        //                          Id = q.unit.Id,
        //                          Name = q.unit.Name
        //                      }
        //                  }
        //              }
        //          }
        //      }).ToListAsync();

        //      return data;
        //  }

        //public async Task<List<DeviceDataDetailDTO>> GetfilterDeviceDataDetails(
        //    int? projectId, int? meterId, int? unitId, DateTime startDate, DateTime endDate, string? timeRange)
        //{
        //    // ✅ Ensure date range is valid
        //    if (startDate < new DateTime(1753, 1, 1))
        //        startDate = new DateTime(1753, 1, 1);
        //    if (endDate > new DateTime(9999, 12, 31))
        //        endDate = new DateTime(9999, 12, 31);
        //    var query = from detail in DBEMSContext.DeviceDataDetails
        //                join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
        //                join device in DBEMSContext.Devices on Convert.ToInt32(master.DeviceId) equals device.Id
        //                join gateway in DBEMSContext.Gateways on device.FkGatewayId equals gateway.Id
        //                join unit in DBEMSContext.Units on gateway.FkUnitId equals unit.Id
        //                join project in DBEMSContext.ProjectManagements on unit.FkProjectManagement equals project.Id
        //                where detail.CreatedAt >= startDate && detail.CreatedAt <= endDate
        //                select new { detail, master, device, gateway, unit, project };

        //    // ✅ Filters (Jo parameters milain unko apply karo)
        //    if (meterId.HasValue)
        //        query = query.Where(q => q.device.Id == meterId);

        //    if (unitId.HasValue)
        //        query = query.Where(q => q.unit.Id == unitId);

        //    if (projectId.HasValue)
        //        query = query.Where(q => q.project.Id == projectId);

        //    // ✅ Time Range Grouping
        //    switch (timeRange?.ToLower())
        //    {
        //        case "daily":
        //            query = query.GroupBy(q => q.detail.CreatedAt.Value.Date)
        //                         .Select(g => g.FirstOrDefault());
        //            break;
        //        case "hourly":
        //            query = query.GroupBy(q => new { q.detail.CreatedAt.Value.Date, q.detail.CreatedAt.Value.Hour })
        //                         .Select(g => g.FirstOrDefault());
        //            break;
        //        case "weekly":
        //            query = query.GroupBy(q => new { Year = q.detail.CreatedAt.Value.Year, Week = EF.Functions.DateDiffWeek(new DateTime(1900, 1, 1), q.detail.CreatedAt) })
        //                         .Select(g => g.FirstOrDefault());
        //            break;
        //        case "monthly":
        //            query = query.GroupBy(q => new { q.detail.CreatedAt.Value.Year, q.detail.CreatedAt.Value.Month })
        //                         .Select(g => g.FirstOrDefault());
        //            break;
        //    }

        //    // ✅ DTO Mapping
        //    var data = await query.Select(q => new DeviceDataDetailDTO
        //    {
        //        Id = q.detail.Id,
        //        FkDeviceDataMasterId = q.detail.FkDeviceDataMasterId,
        //        Address = q.detail.Address,
        //        AddressVariable = q.detail.AddressVariable,
        //        CreatedAt = q.detail.CreatedAt,

        //        DeviceDataMaster = new DeviceDataMasterDTO
        //        {
        //            Id = q.master.Id,
        //            DeviceId = q.master.DeviceId,
        //            CreatedAt = q.master.CreatedAt,

        //            Device = new DeviceDTO
        //            {
        //                Id = q.device.Id,
        //                Name = q.device.Name,
        //                SerialNo = q.device.SerialNo,
        //                Status = q.device.Status,

        //                Gateway = new GatewayDTO
        //                {
        //                    Id = q.gateway.Id,
        //                    Name = q.gateway.Name,

        //                    Unit = new UnitDTO
        //                    {
        //                        Id = q.unit.Id,
        //                        Name = q.unit.Name,

        //                        ProjectManagement = new ProjectManagementDTO
        //                        {
        //                            Id = q.project.Id,
        //                            ProjectName = q.project.ProjectName
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }).ToListAsync();

        //    return data;
        //}
        public async Task<List<DeviceDataDetailDTO>> GetfilterDeviceDataDetails(
      int? projectId, int? meterId, int? unitId, DateTime startDate, DateTime endDate, string? timeRange)
        {
            var query = from detail in DBEMSContext.DeviceDataDetails
                        join master in DBEMSContext.DeviceDataMasters on detail.FkDeviceDataMasterId equals master.Id
                        join device in DBEMSContext.Devices on Convert.ToInt32(master.DeviceId) equals device.Id
                        join gateway in DBEMSContext.Gateways on device.FkGatewayId equals gateway.Id
                        join unit in DBEMSContext.Units on gateway.FkUnitId equals unit.Id
                        join project in DBEMSContext.ProjectManagements on unit.FkProjectManagement equals project.Id
                        where detail.CreatedAt >= startDate && detail.CreatedAt <= endDate
                        select new { detail, master, device, gateway, unit, project };

            // ✅ Filters
            if (meterId.HasValue)
                query = query.Where(q => q.device.Id == meterId);
            if (unitId.HasValue)
                query = query.Where(q => q.unit.Id == unitId);
            if (projectId.HasValue)
                query = query.Where(q => q.project.Id == projectId);

            // ✅ Fetch Data First (To Avoid Translation Issues)
            var dataList =  query.AsEnumerable().ToList(); // ✅ Data pehle fetch karlo

            // ✅ Time Range Grouping (Client-Side After Fetch)
            switch (timeRange?.ToLower())
            {
                case "daily":
                    dataList = dataList.GroupBy(q => q.detail.CreatedAt?.Date)
                                       .Select(g => g.First())
                                       .ToList();
                    break;
                case "hourly":
                    dataList = dataList.GroupBy(q => new { q.detail.CreatedAt?.Date, q.detail.CreatedAt?.Hour })
                                       .Select(g => g.First())
                                       .ToList();
                    break;
                case "weekly":
                    dataList = dataList.GroupBy(q => new
                    {
                        Year = q.detail.CreatedAt?.Year,
                        Week = EF.Functions.DateDiffWeek(new DateTime(1900, 1, 1), q.detail.CreatedAt)
                    })
                    .Select(g => g.First())
                    .ToList();
                    break;
                case "monthly":
                    dataList = dataList.GroupBy(q => new { q.detail.CreatedAt?.Year, q.detail.CreatedAt?.Month })
                                       .Select(g => g.First())
                                       .ToList();
                    break;
            }

            // ✅ DTO Mapping
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
                    CreatedAt = q.master.CreatedAt,

                    Device = new DeviceDTO
                    {
                        Id = q.device.Id,
                        Name = q.device.Name,
                        SerialNo = q.device.SerialNo,
                        Status = q.device.Status,

                        Gateway = new GatewayDTO
                        {
                            Id = q.gateway.Id,
                            Name = q.gateway.Name,

                            Unit = new UnitDTO
                            {
                                Id = q.unit.Id,
                                Name = q.unit.Name,

                                ProjectManagement = new ProjectManagementDTO
                                {
                                    Id = q.project.Id,
                                    ProjectName = q.project.ProjectName
                                }
                            }
                        }
                    }
                }
            }).ToList();

            return result;
        }



    }
}
