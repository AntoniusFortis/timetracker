using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Timetracker.Entities.Models
{
    public class User
    {
        public int Id { get; set; }
 
        public string Login { get; set; }

        [StringLength(200)]
        [JsonIgnore]
        public string Pass { get; set; }

        [JsonIgnore]
        public byte[] Salt { get; set; }
    }
}
