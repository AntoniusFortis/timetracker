using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Timetracker.Entities.Models
{
    public class WorkTask
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public string Title { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Duration { get; set; }

        public byte StateId { get; set; }

        public State State { get; set; }

        [JsonIgnore]
        public List<Worktrack> WorkTracks { get; set; }
    }
}
