using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
