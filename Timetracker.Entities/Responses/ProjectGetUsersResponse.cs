using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Timetracker.Models.Responses
{
    public class ProjectGetUsersResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public dynamic users { get; set; }
    }
}
