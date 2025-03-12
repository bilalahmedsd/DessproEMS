using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class DeviceDataMasterDTO:BaseDTO
    {
        public string? DeviceId { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DeviceDTO? Device { get; set; }
    }
}
