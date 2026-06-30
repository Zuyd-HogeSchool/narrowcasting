using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Narrowcasting.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Department? Department { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<MediaFile> UploadedFiles { get; set; } = new List<MediaFile>();
        public ICollection<Playlist> CreatedPlaylists { get; set; } = new List<Playlist>();
        public ICollection<Announcement> CreatedAnnouncements { get; set; } = new List<Announcement>();
    }
}
