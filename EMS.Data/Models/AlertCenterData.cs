using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Data.Models
{
    public partial class AlertCenterData
    {
        public int Id { get; set; }
        public int? FkDeviceId { get; set; }
        public int? FkUnitId { get; set; }
        public string Address { get; set; }
        public int Min { get; set; }

        public int Max { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
