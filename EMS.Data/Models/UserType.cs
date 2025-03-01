using System;
using System.Collections.Generic;

namespace EMS.Data.Models;

public partial class UserType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }
}
