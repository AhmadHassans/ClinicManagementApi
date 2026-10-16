using ClinicApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Doctor" },
                new Role { Id = 3, Name = "Receptionist" }
            );

            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "Write Prescription", Code = "Prescription.Write" },
                new Permission { Id = 2, Name = "Create Appointment", Code = "Appointment.Create" },
                new Permission { Id = 3, Name = "Delete Appointment", Code = "Appointment.Delete" }
            );

            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 1, PermissionId = 1 },
                new RolePermission { RoleId = 1, PermissionId = 2 },
                new RolePermission { RoleId = 1, PermissionId = 3 },
                new RolePermission { RoleId = 2, PermissionId = 1 },
                new RolePermission { RoleId = 3, PermissionId = 2 }
            );
            
            
        }
    }
}