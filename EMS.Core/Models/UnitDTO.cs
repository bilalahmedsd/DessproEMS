using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class UnitDTO : BaseDTO
    {
        public string? Name { get; set; }

        public bool? IsActive { get; set; } 

    public bool? IsDeleted { get; set; } = false;

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? FkProjectManagement { get; set; }

    public string? SerialNumber { get; set; }

    public string? Status { get; set; }
        public int? FkCompanyId { get; set; } 
        public ProjectManagementDTO? ProjectManagement { get; set; }
    //public List<DeviceDTO?> Meters { get; set; }
    }
}
