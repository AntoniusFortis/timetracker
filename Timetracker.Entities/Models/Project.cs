using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class Project
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
    }
}
