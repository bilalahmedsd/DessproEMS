using System;
using System.Collections.Generic;

namespace EMS.Data.Models;

public partial class ProjectManagement
{
    public int Id { get; set; }

    public int? FkCompanyId { get; set; }

    public string? ProjectName { get; set; }

    public string? CustomerName { get; set; }

    public string? Address { get; set; }

    public string? Principal { get; set; }

    public bool? IsDeleted { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }
}
