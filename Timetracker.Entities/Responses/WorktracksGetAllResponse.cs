/* Автор: Антон Другалев  
* Проект: Timetracker.Models
*/

namespace Timetracker.Models.Responses
{
    using System;
    using System.Net;

    public class WorktracksGetAllResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public int id { get; set; }

        public string login { get; set; }

        public DateTime startedTime { get; set; }

        public DateTime stoppedTime { get; set; }

        public string totalTime { get; set; }
    }
}
