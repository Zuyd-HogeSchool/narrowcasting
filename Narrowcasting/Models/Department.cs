using System.ComponentModel.DataAnnotations;

namespace Narrowcasting.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        //Navigation
        public ICollection<Screen> Screens { get; set; } = new List<Screen>();
    }
}
