using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class RoleRightMapping
{
    public int RoleRightId { get; set; }

    public int? RoleId { get; set; }

    public int? RightId { get; set; }
}
