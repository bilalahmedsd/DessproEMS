using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class FilteredConsumptionDeviceDataDetailDTO
    {
    }
    public class ProjectDataRequestMainMeter
    {
        public int ProjectId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string TimeRange { get; set; }
        public List<UnitRequest> Units { get; set; }
    }

    public class UnitRequest
    {
        public int UnitId { get; set; }
        public int EPI { get; set; }
        public int EPE { get; set; }
    }
}
