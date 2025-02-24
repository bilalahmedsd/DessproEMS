using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Server;
using System.Text.Json;
using System.Text;
using EMS.Core.Interfaces;
using System.Diagnostics;

namespace EMS.CronJobs

{
    public class MqttCronJob(IFourFaith fourFaith) : BackgroundService
    {
        private MqttServer? mqttServer;
        
        private readonly IFourFaith _FourFaith = fourFaith;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttFactory = new MqttFactory();
            var mqttServerOptions = new MqttServerOptionsBuilder()
             .WithDefaultEndpoint()
             .WithDefaultEndpointPort(1883) 
             .Build();
            mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);

            mqttServer.ValidatingConnectionAsync += e =>
            {
                Console.WriteLine($"New connection: ClientId={e.ClientId}");
                return Task.CompletedTask;
            };

            mqttServer.InterceptingPublishAsync += e =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                try
                {
                    if (e.ApplicationMessage.Topic == "energy_meter/data")
                    {
                        _FourFaith.Save(message);
                        Debug.WriteLine("Saved");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing JSON: {ex.Message}");
                }

                return Task.CompletedTask;
            };

            await mqttServer.StartAsync();


        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {

            await mqttServer.StopAsync();

            await base.StopAsync(stoppingToken);
        }

    }
}
