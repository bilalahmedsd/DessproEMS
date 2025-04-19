using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class RealTimeDataDeviceUnitWise
    {
        public string MinuteBlockString { get; set; } // Format: "yyyy-MM-dd HH:mm"
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public double? TotalAddressVariable { get; set; }
    }
}
