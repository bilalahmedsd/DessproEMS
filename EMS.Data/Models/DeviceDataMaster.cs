using System;
using System.Collections.Generic;

namespace EMS.Data.Models;

public partial class DeviceDataMaster
{
    public int Id { get; set; }

    public string? DeviceId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? FkDeviceId { get; set; }

    public int? FkGatewayId { get; set; }

    public int? FkCompanyId { get; set; }

    public virtual Device Device { get; set; }
    public virtual ICollection<DeviceDataDetail> DeviceDataDetails { get; set; } = new List<DeviceDataDetail>();

}
