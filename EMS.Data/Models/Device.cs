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

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? FkCompanyId { get; set; }
}
