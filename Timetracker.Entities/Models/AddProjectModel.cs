using System.ComponentModel.DataAnnotations;

namespace Timetracker.Models.Models
{
    public class AddProjectModel
    {
        [Required]
        public string title { get; set; }

        public string description { get; set; }
    }
}
