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

        public DateTime StartedTime { get; set; }

        public DateTime StoppedTime { get; set; }

        public int TaskId { get; set; }

        public WorkTask Task { get; set; }

        public bool Draft { get; set; }
    }
}
