using System.ComponentModel.DataAnnotations;

namespace Narrowcasting.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }


        public bool IsInternal { get; set; } = false;

        // FK
        public int DepartmentId { get; set; }
        public int? ScreenId { get; set; }
        public int? MediaFileId { get; set; }
        public string? CreatedById { get; set; }

        // Navigation properties
        public Department? Department { get; set; }
        public Screen? Screen { get; set; }
        public MediaFile? MediaFile { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
    }
}
