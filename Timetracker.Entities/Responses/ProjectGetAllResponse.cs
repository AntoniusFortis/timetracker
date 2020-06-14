using System.Collections.Generic;
using System.Net;
using Timetracker.Entities.Models;

namespace Timetracker.Models.Responses
{
    public class ProjectGetAllResponse
    {
       public HttpStatusCode status { get; set; }

       public List<Project> acceptedProjects { get; set; }

       public List<Project> notAcceptedProjects { get; set; }
    }
}
