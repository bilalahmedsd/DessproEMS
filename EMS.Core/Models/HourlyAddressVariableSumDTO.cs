using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class HourlyAddressVariableSumDTO
    {
        public int Hour { get; set; } // 0 to 23
        public int UnitId { get; set; }

        public int MinuteBlock { get; set; }
        public string UnitName { get; set; }
        public double? TotalAddressVariable { get; set; }
    }
}
