using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class UserRoleMapping
{
    public int UserRoleId { get; set; }

    public int UserId { get; set; }

    public string RoleId { get; set; } = null!;

}
