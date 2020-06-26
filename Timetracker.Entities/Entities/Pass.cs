/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Models.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

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
