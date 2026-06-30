using System.ComponentModel.DataAnnotations;

namespace Narrowcasting.Models
{
    public class Screen
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsStaffScreen { get; set; } = false;

        //FK
        [Range(1, int.MaxValue, ErrorMessage = "Selecteer een afdeling.")]
        public int DepartmentId { get; set; }

        //Navigation properties
        //Make navigation nullable so model validation does not require the full related entity
        public Department? Department { get; set; }
        public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

    }
}
