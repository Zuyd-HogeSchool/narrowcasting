using System.ComponentModel.DataAnnotations;

namespace Narrowcasting.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string EntityType { get; set; } = string.Empty; // e.g. "Screen", "Playlist", "Announcement"

        public string? EntityId { get; set; }

        public DateTime Timestamp { get; set; }

        [MaxLength(2000)]
        public string? Details { get; set; }

        // FK
        public string? UserId { get; set; }

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}
