using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models
{
    public class DeviceDTO : BaseDTO
    {
        public int? FkGatewayId { get; set; }

        public string? Name { get; set; }

        public string? ChannelName { get; set; }

        public string? ConsumptionUnit { get; set; }

        public int? DisplaySequence { get; set; }

        public string? SerialPort { get; set; }

        public string? SerialNo { get; set; }

        public string? Status { get; set; }

        public DateTime? OfflineTime { get; set; }

        public string? OfflineDuration { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsDeleted { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? FkCompanyId { get; set; }

        public GatewayDTO? Gateway { get; set; }
        public int? FkUnitId { get; set; }

        public UnitDTO? Unit { get; set; }
        public List<DeviceDataDetailDTO> DeviceDataDetails { get; set; }

    }
}
