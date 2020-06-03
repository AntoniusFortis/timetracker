using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Timetracker.Entities.Models
{
    public class AuthorizedUser
    {
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.Identity )]
        public int Id { get; set; }

        public byte RightId { get; set; }

        public virtual Right Right { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public int ProjectId { get; set; }

        [JsonIgnore]
        public virtual Project Project { get; set; }

        public bool IsSigned { get; set; }
    }
}
