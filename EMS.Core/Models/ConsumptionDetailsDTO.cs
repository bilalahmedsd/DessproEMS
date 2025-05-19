using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
   
        public class ConsumptionDetailsDTO : BaseDTO
        {
            public DateTime? SelectedDate { get; set; }
            public int? SelectedUnit { get; set; }
            public int? SelectedDevice { get; set; }
            public string? SelectedRange { get; set; }

            public string? Address { get; set; }

            public double? AddressVariable { get; set; }

        }
    
}
