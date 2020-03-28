namespace Timetracker.Entities.Models
{
    public class AuthorizedUser
    {
        public int Id { get; set; }

        public byte RightId { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public bool IsSigned { get; set; }
    }
}
