using System.ComponentModel.DataAnnotations;
using Narrowcasting.Enums;

namespace Narrowcasting.Models
{
    public class MediaFile
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(10000)]
        public string? TextContent { get; set; }

        [MaxLength(500)]
        public string? Caption { get; set; }

        public MediaType MediaType { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        //FK
        public string? UploadedById { get; set; }

        //Navigation properties
        public ApplicationUser? UploadedBy { get; set; }
        public ICollection<PlaylistItem> PlaylistItems { get; set; } = new List<PlaylistItem>();
    }
}
