using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class ReportModel
    {
        [Required]
        public int projectId { get; set; }

        public int? userId { get; set; }

        public int? taskId { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }
    }
}
