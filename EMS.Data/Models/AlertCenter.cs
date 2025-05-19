using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Data.Models
{
    public partial class AlertCenter
    {
        public int id { get; set; }
        public int? FkDeviceId { get; set; }
        public int? FkUnitId { get; set; }
        public string AlertLevel { get; set; }
        public string Event { get; set; }
        public DateTime createdAt { get; set; }

        public bool isDeleted { get; set; } = false;
    }
}
