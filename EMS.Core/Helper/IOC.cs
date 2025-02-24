using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Helper
{
    public static class IoC
    {
        private static IServiceProvider Services { get; set; }
        public static T Get<T>() => (T)Services.GetService(typeof(T));

        public static void InitServices(IServiceProvider services)
        {
            Services = services;
        }
    }
}
