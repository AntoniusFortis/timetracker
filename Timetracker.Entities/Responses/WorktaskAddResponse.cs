using System.Net;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Responses
{
    public class WorktaskAddResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public WorkTask worktask { get; set; }
    }
}
