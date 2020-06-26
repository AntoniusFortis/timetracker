using System.Collections.Generic;
using System.Net;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Responses
{
    public class StateGetAllResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public List<State> states { get; set; }
    }
}
