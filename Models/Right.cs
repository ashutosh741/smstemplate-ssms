using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Right
{
    public int RightId { get; set; }

    public string? RightName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}
