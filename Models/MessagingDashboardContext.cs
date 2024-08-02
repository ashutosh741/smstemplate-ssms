using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class MessagingDashboardContext : DbContext
{
    public MessagingDashboardContext()
    {
    }

    public MessagingDashboardContext(DbContextOptions<MessagingDashboardContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Right> Rights { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleRightMapping> RoleRightMappings { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    public virtual DbSet<UserRoleMapping> UserRoleMappings { get; set; }
    public virtual DbSet<smsTemplate> SmsTemplate { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { 
        modelBuilder.Entity<Right>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.RightName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role"); 

            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RoleRightMapping>(entity =>
        {
            entity.HasKey(e => e.RoleRightId);

            entity.ToTable("RoleRightMapping");
        });

        modelBuilder.Entity<Users>(entity =>    
        
        
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired(false);
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserRoleMapping>(entity =>
        {
            entity.HasKey(e => e.UserRoleId);

            entity.ToTable("UserRoleMapping");

            entity.Property(e => e.RoleId)
                .HasMaxLength(10)
                .IsFixedLength();
        });
        modelBuilder.Entity<smsTemplate>(entity =>
        {
            entity.HasKey(e => e.tid);

            entity.Property(e => e.templateid)
                .HasMaxLength(50)  // Adjust the length if needed
                .IsUnicode(false);

            entity.Property(e => e.Content)
                .HasMaxLength(500)  // Adjust the length if needed
                .IsUnicode(false);

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.CreatedDateTime)
                .HasColumnType("datetime");

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.UpdatedDateTime)
                .HasColumnType("datetime");

            entity.Property(e => e.Status)
                .HasColumnType("string");  // For nullable boolean values
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
