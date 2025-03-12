using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class DeviceDataDetailDTO : BaseDTO
    {
        public int? FkDeviceDataMasterId { get; set; }

        public string? Address { get; set; }

        public double? AddressVariable { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DeviceDataMasterDTO? DeviceDataMaster { get; set; }
    }
}
