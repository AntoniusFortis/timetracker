using System.ComponentModel.DataAnnotations;

namespace Timetracker.Models.Models
{
    public class AddProjectModel
    {
        [Required]
        [StringLength( maximumLength: 50, MinimumLength = 5 )]
        public string title { get; set; }

        [StringLength( maximumLength: 250, MinimumLength = 1 )]
        public string description { get; set; }
    }
}
