using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class GatewayDTO : BaseDTO
    {
        
        public string? Name { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsDeleted { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? ProtocolName { get; set; }

        public string? SerialNo { get; set; }

        public int? AccumulatedVariable { get; set; }

        public int? InstantVariable { get; set; }
        public int? FkCompanyId { get; set; }
        public int? FkUnitId { get; set; }

        public UnitDTO? Unit { get; set; }
    }
}
