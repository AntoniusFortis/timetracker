using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Timetracker.Entities.Models
{
    public class WorkTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength( 50 )]
        public string Title { get; set; }

        [StringLength( 250 )]
        public string Description { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Duration { get; set; }

        public byte StateId { get; set; }

        public State State { get; set; }

        [JsonIgnore]
        public List<Worktrack> WorkTracks { get; set; }
    }
}
