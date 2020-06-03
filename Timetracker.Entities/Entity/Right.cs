using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class Right
    {
        public byte Id { get; set; }

        [StringLength( 50 )]
        public string Name { get; set; }
    }
}
