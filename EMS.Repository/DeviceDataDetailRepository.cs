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

        public async Task<List<DeviceDataDetailDTO>> GetDeviceDataDetailsAsync() {
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
            return  data.ToJson().FromJson<List<DeviceDataDetailDTO>>();
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

    }
}
