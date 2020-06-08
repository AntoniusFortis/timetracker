using System.Net;
using Timetracker.Entities.Models;

namespace Timetracker.Models.Responses
{
    public class WorktaskDeleteResponse
    {
        public HttpStatusCode status = HttpStatusCode.OK;

        public WorkTask worktask { get; set; }
    }
}
