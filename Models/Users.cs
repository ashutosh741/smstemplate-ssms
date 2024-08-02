using System;

namespace WebApplication1.Models
{
    public class Users
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = null!;

        public required string Password { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }

    }
}
