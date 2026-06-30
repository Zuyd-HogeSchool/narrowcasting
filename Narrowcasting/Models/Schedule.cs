using NarrowCasting.Enums;

namespace Narrowcasting.Models
{
    public class Schedule
    {
        public int Id { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public bool IsRecurring { get; set; } = false;
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

        // FK
        public int PlaylistId { get; set; }
        public int? ScreenId { get; set; }

        // Navigation properties
        public Playlist Playlist { get; set; } = null!;
        public Screen? Screen { get; set; }
    }
}
