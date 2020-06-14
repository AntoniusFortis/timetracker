using System.Net;

namespace Timetracker.Models.Responses
{
    public class SignInResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public string access_token { get; set; }

        public string refresh_token { get; set; }

        public double expired_in { get; set; }

        public object user { get; set; }
    }
}
