using EMS.CronJobs;
using EMS.CronJobs.ForFaith;
using EMS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .UseWindowsService()
    .ConfigureServices((context, services) =>
    {
        string connStr = context.Configuration.GetConnectionString("EMS");

        services.AddDbContext<EMSContext>(options =>
            options.UseSqlServer(connStr, sql => sql.CommandTimeout(300)));
        services.AddSingleton<FourFaith>();
        services.AddHostedService<MqttCronJob>();
    })
    .Build()
    .Run();
Console.WriteLine("Running...");