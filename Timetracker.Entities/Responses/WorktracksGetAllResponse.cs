using System;
using System.Net;

namespace Timetracker.Models.Responses
{
    public class WorktracksGetAllResponse
    {
        public HttpStatusCode status = HttpStatusCode.OK;

        public int id { get; set; }

        public string login { get; set; }

        public DateTime startedTime { get; set; }

        public DateTime stoppedTime { get; set; }

        public string totalTime { get; set; }
    }
}
