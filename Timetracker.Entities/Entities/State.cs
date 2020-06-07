using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class State
    {
        public byte Id { get; set; }

        [Required]
        [StringLength( 60 )]
        public string Title { get; set; }
    }
}