using System.Net;

namespace Timetracker.Models.Responses
{
    public class MypageGetResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public object user { get; set; }
    }
}
