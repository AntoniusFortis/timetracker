using System.Net;

namespace Timetracker.Models.Responses
{
    public class MypageGetResponse
    {
        public HttpStatusCode status = HttpStatusCode.OK;

        public object user { get; set; }
    }
}
