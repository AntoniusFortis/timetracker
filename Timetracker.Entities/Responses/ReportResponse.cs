using System;
using System.Net;

namespace Timetracker.Models.Responses
{
    public class ReportResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public int id { get; set; }

        public string login { get; set; }

        public string task { get; set; }

        public int taskId { get; set; }

        public DateTime startedTime { get; set; }

        public DateTime stoppedTime { get; set; }

        public string totalTime { get; set; }
    }
}
