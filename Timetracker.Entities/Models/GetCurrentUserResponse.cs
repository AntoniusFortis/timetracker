using System.Net;
using Timetracker.Entities.Models;

namespace Timetracker.Models.Models
{
    public class GetCurrentUserResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public User user { get; set; }
    }
}
