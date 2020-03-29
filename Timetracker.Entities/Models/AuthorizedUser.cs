namespace Timetracker.Entities.Models
{
    public class AuthorizedUser
    {
        public int Id { get; set; }

        public byte RightId { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public int ProjectId { get; set; }

        public virtual Project Project { get; set; }

        public bool IsSigned { get; set; }
    }
}
