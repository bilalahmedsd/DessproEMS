using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class EPIConspDTO : BaseDTO
    {
        public int? UnitId { get; set; }

        public List<EPIConspData> Data { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class EPIConspData
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}