using EMS.Core.Helpers;
using EMS.Core.Interfaces;
using EMS.Core.Models.ForFaith;
using EMS.Data.Models;
using EMS.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.CronJobs.ForFaith
{
    public class FourFaith : IFourFaith
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public FourFaith(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public async Task Save(string data)
        {
            if (data != null)
            {
                using (var scope = _scopeFactory.CreateScope()) // Create a new scope
                {
                    var DBEMSContext = scope.ServiceProvider.GetRequiredService<EMSContext>();
                    var energyData = data.FromJson<FourFaithMqttPayloadDTO>();
                   
                    var gateway = await DBEMSContext.Gateways.Where(x => x.SerialNo == energyData.did && x.IsDeleted == false && x.IsActive == true).FirstOrDefaultAsync();
                    if (gateway != null)
                    {
                        var master = new DeviceDataMaster
                        {
                            DeviceId = energyData.did,
                            CreatedAt = DateTime.Now,
                            FkGatewayId = gateway.Id,
                            FkCompanyId = gateway.FkCompanyId
                        };
                        await DBEMSContext.DeviceDataMasters.AddAsync(master);
                        await DBEMSContext.SaveChangesAsync();

                        

                        foreach (var item in energyData.content)
                        {
                            await DBEMSContext.DeviceDataDetails.AddAsync(new DeviceDataDetail()
                            {
                                Address = item.addr,
                                AddressVariable = item.addrv,
                                CreatedAt = DateTime.Now,
                                FkDeviceDataMasterId = master.Id

                            });
                        }
                        await DBEMSContext.SaveChangesAsync();
                    }
                }

            }
            await Task.CompletedTask;
        }


    }
}
