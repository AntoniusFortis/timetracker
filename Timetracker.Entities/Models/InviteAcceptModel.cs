using System.ComponentModel.DataAnnotations;

namespace Timetracker.Models.Models
{
    public class InviteAcceptModel
    {
        [Required]
        public int projectId { get; set; }
    }
}
