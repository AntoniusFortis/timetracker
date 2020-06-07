using System.Net;

namespace Timetracker.Models.Responses
{
    public class OkResponse
    {
        public HttpStatusCode status = HttpStatusCode.OK;

        public string message { get; set; }
    }
}
