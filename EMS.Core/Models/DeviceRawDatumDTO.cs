using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class DeviceRawDatumDTO : BaseDTO
    {
        public string? Data { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
