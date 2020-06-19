using System.Collections.Generic;
using System.Net;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Responses
{
    public class ProjectGetAllResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

       public List<Project> acceptedProjects { get; set; }

       public List<Project> notAcceptedProjects { get; set; }
    }
}
