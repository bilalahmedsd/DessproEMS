using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class UnitWiseAddressVariableSumDTO : BaseDTO
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public double? TotalAddressVariable { get; set; }

        public DateTime? Date { get; set; }
    }
}
