using System.Net;

namespace Timetracker.Models.Responses
{
    public class ErrorResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.BadRequest;

        public string message { get; set; }
    }
}
