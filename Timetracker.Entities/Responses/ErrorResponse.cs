using System.Net;

namespace Timetracker.Models.Response
{
    public class ErrorResponse
    {
        public HttpStatusCode status = HttpStatusCode.BadRequest;

        public string message { get; set; }
    }
}
