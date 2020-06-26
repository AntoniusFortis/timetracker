using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Responses
{
    public class ProjectAcceptResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public Project project { get; set; }
    }
}
