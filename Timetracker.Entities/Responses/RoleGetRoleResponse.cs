using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Timetracker.Models.Responses
{
    public class RoleGetRoleResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public byte role { get; set; }

        public bool isAdmin { get; set; }
    }
}
