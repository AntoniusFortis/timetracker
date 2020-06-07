using System.Net;

namespace Timetracker.Models.Responses
{
    public class TokenResponse
    {
        public HttpStatusCode status { get; set; }
        
        public string access_token { get; set; }

        public string refresh_token { get; set; }
        
        public double expired_in { get; set; }
    }
}
