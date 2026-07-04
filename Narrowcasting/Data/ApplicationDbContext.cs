using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Narrowcasting.Models;

namespace Narrowcasting.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Screen> Screens => Set<Screen>();
        public DbSet<Playlist> Playlists => Set<Playlist>();
        public DbSet<PlaylistItem> PlaylistItems => Set<PlaylistItem>();
        public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
        public DbSet<Schedule> Schedules => Set<Schedule>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser
            builder.Entity<ApplicationUser>(e =>
            {
                e.Property(u => u.FullName).IsRequired().HasMaxLength(150);
                e.HasOne(u => u.Department)
                 .WithMany(d => d.Users)
                 .HasForeignKey(u => u.DepartmentId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // Department
            builder.Entity<Department>(e =>
            {
                e.HasKey(d => d.Id);
                e.Property(d => d.Name).IsRequired().HasMaxLength(100);
                e.Property(d => d.Description).HasMaxLength(500);
            });

            // Screen
            builder.Entity<Screen>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.Name).IsRequired().HasMaxLength(100);
                e.Property(s => s.Location).IsRequired().HasMaxLength(200);

                e.HasOne(s => s.Department)
                 .WithMany(d => d.Screens)
                 .HasForeignKey(s => s.DepartmentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Playlist
            builder.Entity<Playlist>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Name).IsRequired().HasMaxLength(100);
                e.Property(p => p.CreatedAt).IsRequired();

                e.HasOne(p => p.Screen)
                 .WithMany(s => s.Playlists)
                 .HasForeignKey(p => p.ScreenId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(p => p.CreatedBy)
                 .WithMany(u => u.CreatedPlaylists)
                 .HasForeignKey(p => p.CreatedById)
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired(false);
            });

            // PlaylistItem
            builder.Entity<PlaylistItem>(e =>
            {
                e.HasKey(pi => pi.Id);
                e.Property(pi => pi.Order).IsRequired();
                e.Property(pi => pi.DurationSeconds).IsRequired();

                e.HasOne(pi => pi.Playlist)
                 .WithMany(p => p.Items)
                 .HasForeignKey(pi => pi.PlaylistId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(pi => pi.MediaFile)
                 .WithMany(m => m.PlaylistItems)
                 .HasForeignKey(pi => pi.MediaFileId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // MediaFile
            builder.Entity<MediaFile>(e =>
            {
                e.HasKey(mf => mf.Id);
                e.Property(mf => mf.FileName).IsRequired().HasMaxLength(100);
                e.Property(mf => mf.FilePath).IsRequired().HasMaxLength(200);
                e.Property(mf => mf.UploadedAt).IsRequired();

                e.HasOne(mf => mf.UploadedBy)
                 .WithMany(u => u.UploadedFiles)
                 .HasForeignKey(mf => mf.UploadedById)
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired(false);
            });

            // Schedule
            builder.Entity<Schedule>(e =>
            {
                e.HasKey(s => s.Id);
                e.HasOne(s => s.Playlist)
                 .WithMany(p => p.Schedules)
                 .HasForeignKey(s => s.PlaylistId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(s => s.Screen)
                 .WithMany(sc => sc.Schedules)
                 .HasForeignKey(s => s.ScreenId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Announcement
            builder.Entity<Announcement>(e =>
            {
                e.HasKey(a => a.Id);
                e.Property(a => a.Title).IsRequired().HasMaxLength(200);
                e.Property(a => a.Content).IsRequired();
                e.Property(a => a.PublishedAt).IsRequired();
                e.Property(a => a.ExpiresAt);
                e.Property(a => a.IsInternal).IsRequired();

                e.HasOne(a => a.Department)
                 .WithMany(d => d.Announcements)
                 .HasForeignKey(a => a.DepartmentId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Screen)
                 .WithMany()
                 .HasForeignKey(a => a.ScreenId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired(false);

                e.HasOne(a => a.MediaFile)
                 .WithMany()
                 .HasForeignKey(a => a.MediaFileId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);

                e.HasOne(a => a.CreatedBy)
                 .WithMany(u => u.CreatedAnnouncements)
                 .HasForeignKey(a => a.CreatedById)
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired(false);
            });

            // AuditLog
            builder.Entity<AuditLog>(e =>
            {
                e.HasKey(al => al.Id);
                e.Property(al => al.Action).IsRequired().HasMaxLength(100);
                e.Property(al => al.EntityType).IsRequired().HasMaxLength(100);
                e.Property(al => al.Details).HasMaxLength(2000);

                e.HasOne(al => al.User)
                 .WithMany(u => u.AuditLogs)
                 .HasForeignKey(al => al.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            SeedData(builder);
        }

        private static void SeedData(ModelBuilder builder)
        {
            // Departments
            builder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Wachtkamer", Description = "Algemene wachtkamer begane grond" },
                new Department { Id = 2, Name = "Radiologie", Description = "Radiologie & beeldvorming" },
                new Department { Id = 3, Name = "Apotheek", Description = "Ziekenhuisapotheek" }
            );

            // Screens
            builder.Entity<Screen>().HasData(
                new Screen { Id = 1, Name = "Scherm Wachtkamer A", Location = "Wachtkamer Begane Grond", IsActive = true, DepartmentId = 1 },
                new Screen { Id = 2, Name = "Scherm Radiologie", Location = "Radiologie Gang", IsActive = true, DepartmentId = 2 },
                new Screen { Id = 3, Name = "Scherm Apotheek", Location = "Apotheek Balie", IsActive = true, DepartmentId = 3 }
            );

            // Admin role + Employee role
            var adminRoleId = "ROLE-ADMIN-0001";
            var employeeRoleId = "ROLE-EMP-0001";

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = employeeRoleId, Name = "Employee", NormalizedName = "EMPLOYEE" }
            );

            // Admin user
            //var adminId = "USER-ADMIN-0001";
            //var hasher = new PasswordHasher<ApplicationUser>();
            //var adminUser = new ApplicationUser
            //{
            //    Id = adminId,
            //    UserName = "admin@ziekenhuis.nl",
            //    NormalizedUserName = "ADMIN@ZIEKENHUIS.NL",
            //    Email = "admin@ziekenhuis.nl",
            //    NormalizedEmail = "ADMIN@ZIEKENHUIS.NL",
            //    EmailConfirmed = true,
            //    FullName = "Systeem Administrator",
            //    DepartmentId = null,   // Admin = cross-department
            //    IsActive = true,
            //    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //};
            //adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123!");
            //builder.Entity<ApplicationUser>().HasData(adminUser);

            //builder.Entity<IdentityUserRole<string>>().HasData(
            //    new IdentityUserRole<string> { UserId = adminId, RoleId = adminRoleId }
            //);
        }
    }
}