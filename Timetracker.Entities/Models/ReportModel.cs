namespace Timetracker.Entities.Models
{
    public class ReportModel
    {
        public int projectId { get; set; }

        public int? userId { get; set; }

        public int? taskId { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }
    }
}
