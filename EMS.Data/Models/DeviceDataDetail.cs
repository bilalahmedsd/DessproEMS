using System;
using System.Collections.Generic;

namespace EMS.Data.Models;

public partial class DeviceDataDetail
{
    public int Id { get; set; }

    public int? FkDeviceDataMasterId { get; set; }

    public string? Address { get; set; }

    public double? AddressVariable { get; set; }

    public DateTime? CreatedAt { get; set; }
}
