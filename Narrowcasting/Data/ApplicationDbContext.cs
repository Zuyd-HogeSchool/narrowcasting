using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Narrowcasting.Models;

namespace Narrowcasting.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
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
        }

    }
}
