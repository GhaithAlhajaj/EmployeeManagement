
using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence;


public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<Department> Departments { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .UseCollation("NOCASE")
                .HasMaxLength(150);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.Salary)
                .IsRequired();

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name)
                .IsRequired()
                .UseCollation("NOCASE")
                .HasMaxLength(100);

            entity.HasIndex(d => d.Name)
                .IsUnique();

            entity.Property(d => d.Description)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(log => log.Id);
            entity.Property(log => log.ActorEmail)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(log => log.Method)
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(log => log.Path)
                .IsRequired()
                .HasMaxLength(500);
            entity.HasIndex(log => log.TimestampUtc);
            entity.HasIndex(log => log.ActorId);
        });
    }
}
