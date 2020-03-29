using System;

namespace Timetracker.Entities.Models
{
    public class WorkTask
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public string Title { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Duration { get; set; }

        public byte StateId { get; set; }

        public State State { get; set; }
    }
}
