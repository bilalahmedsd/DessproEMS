using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    internal class FilteredDeviceDataDetailDTO
    {
    }
    public class Meter
    {
        public int MeterId { get; set; }
        public int EPI { get; set; }
        public int EQC { get; set; }
        public int EQL { get; set; }
        public int EPE { get; set; }

    }

    public class Unit2
    {
        public int UnitId { get; set; }
        public List<Meter> Meters { get; set; } = new List<Meter>();
    }

    public class ProjectDataRequest
    {
        public int ProjectId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string TimeRange { get; set; }
        public List<Unit2> Units { get; set; } = new List<Unit2>();
    }
}
