using System.ComponentModel.DataAnnotations;

namespace Timetracker.Models.Entities
{
    public class State
    {
        [Key]
        [Required]
        public byte Id { get; set; }

        [Required]
        [StringLength( 60 )]
        public string Title { get; set; }
    }
}