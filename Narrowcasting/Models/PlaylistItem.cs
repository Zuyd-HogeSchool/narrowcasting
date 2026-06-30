using System.ComponentModel.DataAnnotations;

namespace Narrowcasting.Models
{
    public class PlaylistItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Order { get; set; }

        [Range(1, 3600)]
        public int DurationSeconds { get; set; } = 10;

        //FK
        public int PlaylistId { get; set; }
        public int MediaFileId { get; set; }

        //Navigation properties
        public Playlist Playlist { get; set; } = null!;
        public MediaFile MediaFile { get; set; } = null!;
    }
}
