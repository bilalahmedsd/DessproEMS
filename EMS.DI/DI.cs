using EMS.Core.Interfaces;
using EMS.CronJobs.ForFaith;
using EMS.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DI
{
    public static class DI
    {
        public static IServiceCollection AddServices(this IServiceCollection collection)
        {
            //TODO: Add Business Layer Services
            return collection
                .AddTransient<IUsersRepository, UsersRepository>()
                .AddTransient<IDeviceRawDataRepository, DeviceRawDataRepository>()
                .AddTransient<IUnitRepository, UnitRepository>()
                .AddTransient<IProjectManagementRepository,ProjectManagementRepository>()
                .AddSingleton<IFourFaith, FourFaith>();
                
        }
    }
}
