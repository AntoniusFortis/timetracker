using System;
using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class Worktrack
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int WorktaskId { get; set; }

        public WorkTask Worktask { get; set; }

        public DateTime StartedTime { get; set; }

        public DateTime StoppedTime { get; set; }

        public bool Running { get; set; }
    }
}
