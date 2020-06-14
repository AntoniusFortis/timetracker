using System.Net;

namespace Timetracker.Models.Responses
{
    public class ProjectAddResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public object project { get; set; }
    }
}
