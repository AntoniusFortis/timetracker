using Timetracker.Models.Entities;

namespace Timetracker.Models.Responses
{
    public class WorktaskGetResponse
    {
        public Project project { get; set; }

        public object worktask { get; set; }

        public byte isAdmin { get; set; }
    }
}
