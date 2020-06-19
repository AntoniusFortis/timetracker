using System.Net;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Responses
{
    public class WorktaskDeleteResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public WorkTask worktask { get; set; }
    }
}
