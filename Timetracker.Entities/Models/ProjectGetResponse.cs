using System.Collections.Generic;
using System.Net;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Models
{
    public class ProjectGetResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public Project project { get; set; }

        public List<WorkTask> tasks { get; set; }

        public object caller { get; set; }

        public dynamic users { get; set; }
    }
}
