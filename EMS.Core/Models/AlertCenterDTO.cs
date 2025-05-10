using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class AlertCenterDTO
    {

        public int id { get; set; }
        public string AlertLevel { get; set; }
        public string Event { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? UnitId { get; set; }

        public string? UnitName { get; set; }

        public string? DeviceName { get; set; }

        public int? DeviceId { get; set; }

        public bool isDeleted { get; set; }

    }
}
