using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class AuthorizedUser
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        public int RightId { get; set; }

        public int UserId { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public bool IsSigned { get; set; }
    }
}
