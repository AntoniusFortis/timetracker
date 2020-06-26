using System.ComponentModel.DataAnnotations;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Models
{
    public class ProjectUpdateModel
    {
        [Required]
        public Project Project { get; set; }
    }
}
