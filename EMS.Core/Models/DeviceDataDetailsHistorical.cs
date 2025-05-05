using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class DeviceDataDetailsHistorical : BaseDTO
    {
        
        public string? Address { get; set; }

        public double? AddressVariable { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
