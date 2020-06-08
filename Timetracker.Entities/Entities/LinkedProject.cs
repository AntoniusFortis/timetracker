using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Timetracker.Entities.Models
{
    public class LinkedProject
    {
        [Key]
        public int Id { get; set; }

        public byte RightId { get; set; }

        public virtual Role Right { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public int ProjectId { get; set; }

        [JsonIgnore]
        public virtual Project Project { get; set; }

        public bool Accepted { get; set; }
    }
}
