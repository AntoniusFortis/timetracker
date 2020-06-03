﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Timetracker.Entities.Models
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength( 50 )]
        public string Title { get; set; }

        [StringLength( 250 )]
        public string Description { get; set; }

        [JsonIgnore]
        public virtual List<WorkTask> Tasks { get; set; }
    }
}
