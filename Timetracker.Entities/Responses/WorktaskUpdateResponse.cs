/* Автор: Антон Другалев  
* Проект: Timetracker.Models
*/

namespace Timetracker.Models.Responses
{
    using System.Net;
    using Timetracker.Models.Entities;

    public class WorktaskUpdateResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public WorkTask worktask { get; set; }
    }
}
