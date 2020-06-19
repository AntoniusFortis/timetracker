﻿/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Models.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    public class Project
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength( 50 )]
        public string Title { get; set; }

        [StringLength( 250 )]
        public string Description { get; set; }

        [JsonIgnore]
        public virtual List<WorkTask> Tasks { get; set; }
    }
}
