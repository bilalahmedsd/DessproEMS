using System;
using System.Collections.Generic;

namespace EMS.Data.Models;

public partial class Device
{
    public int Id { get; set; }

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

    public int? FkUnitId { get; set; }
    public virtual Unit Unit { get; set; }
    
    public virtual ICollection<DeviceDataMaster> DeviceDataMasters { get; set; } = new List<DeviceDataMaster>();
    public bool? IsMainMeter { get; set; }
}

