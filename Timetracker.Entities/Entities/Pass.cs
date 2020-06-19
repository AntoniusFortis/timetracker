using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Timetracker.Models.Entities
{
    public class Pass
    {
        public int Id { get; set; }

        [StringLength( 500 )]
        [Required]
        [JsonIgnore]
        public string Password { get; set; }

        [Required]
        [JsonIgnore]
        public byte[] Salt { get; set; }
    }
}
