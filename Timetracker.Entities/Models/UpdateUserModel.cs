using System.ComponentModel.DataAnnotations;

namespace Timetracker.Models.Models
{
    public class UpdateUserModel
    {
        [Required]
        public string userLogin { get; set; }

        [Required]
        public string rightId { get; set; }

        [Required]
        public string projectId { get; set; }
    }
}
