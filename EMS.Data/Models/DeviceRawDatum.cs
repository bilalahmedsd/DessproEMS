using System;
using System.Collections.Generic;

namespace EMS.Data.Models;

public partial class DeviceRawDatum
{
    public int Id { get; set; }

    public string? Data { get; set; }

    public DateTime? CreatedAt { get; set; }
}
