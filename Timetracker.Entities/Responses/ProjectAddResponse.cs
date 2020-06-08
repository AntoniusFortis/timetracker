using System.Net;

namespace Timetracker.Models.Responses
{
    public class ProjectAddResponse
    {
        public HttpStatusCode status = HttpStatusCode.OK;

        public object project { get; set; }
    }
}
