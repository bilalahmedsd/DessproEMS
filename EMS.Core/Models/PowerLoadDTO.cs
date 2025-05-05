using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class PowerLoadDTO
    {
      

        public double? AddressVariable { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? UnitName { get; set; }

        public string? DayName => CreatedAt?.ToString("dddd");
    }
}
