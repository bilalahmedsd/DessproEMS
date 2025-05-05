
using EMS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace EMS.CronJobs.ForFaith
{
    public class FourFaith 
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
                    var energyData = JsonConvert.DeserializeObject<FourFaithMqttPayloadDTO>(data);
                   
                    var gateway = await DBEMSContext.Gateways.Where(x => x.SerialNo == energyData.did && x.IsDeleted == false && x.IsActive == true).FirstOrDefaultAsync();
                    if (gateway != null)
                    {
                        var Sn = energyData.content.Where(x => x.addr.ToString().Contains("SN")).FirstOrDefault();
                        if (Sn != null && Regex.IsMatch(Sn.addr, @"^SN-\d{4}$"))
                        {
                            var device = await DBEMSContext.Devices.Where(x => "SN-" + x.SerialNo == Sn.addr).FirstOrDefaultAsync();
                            if (device != null)
                            {
                                var master = new DeviceDataMaster
                                {
                                    DeviceId = energyData.did,
                                    CreatedAt = DateTime.Now,
                                    FkGatewayId = gateway.Id,
                                    FkCompanyId = gateway.FkCompanyId,
                                    FkDeviceId = device.Id
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
                    else
                    { }
                }

            }
            await Task.CompletedTask;
        }


    }
}
