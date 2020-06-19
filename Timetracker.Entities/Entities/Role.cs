using System.ComponentModel.DataAnnotations;

namespace Timetracker.Models.Entities
{
    public class Role
    {
        public byte Id { get; set; }

        [Required]
        [StringLength( 50 )]
        public string Title { get; set; }
    }
}
