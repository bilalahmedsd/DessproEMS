using System;
using System.Collections.Generic;

namespace EMS.Data.Models;

public partial class Unit
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? FkProjectManagement { get; set; }

    public string? SerialNumber { get; set; }

    public string? Status { get; set; }
}
