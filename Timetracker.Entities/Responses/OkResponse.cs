using System.Net;

namespace Timetracker.Models.Responses
{
    public class OkResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public string message { get; set; }
    }
}
