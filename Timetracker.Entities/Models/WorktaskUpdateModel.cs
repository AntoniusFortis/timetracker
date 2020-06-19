using System.ComponentModel.DataAnnotations;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Models
{
    public class WorktaskUpdateModel
    {
        [Required]
        public WorkTask worktask { get; set; }
    }
}