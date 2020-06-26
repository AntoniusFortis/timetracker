using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Responses
{
    public class RoleGetAllResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public List<Role> roles { get; set; }
    }
}
