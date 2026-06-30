using System.ComponentModel.DataAnnotations;

namespace Narrowcasting.Models
{
    public class Playlist
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //FK
        [Range(1, int.MaxValue, ErrorMessage = "Selecteer een scherm")]
        public int ScreenId { get; set; }
        public string CreatedById { get; set; } = string.Empty;

        //Navigation
        public Screen? Screen { get; set; }
        public ICollection<PlaylistItem> PlaylistItems { get; set; } = new List<PlaylistItem>();
    }
}
