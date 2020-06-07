using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class Role
    {
        public byte Id { get; set; }

        [Required]
        [StringLength( 50 )]
        public string Title { get; set; }
    }
}
