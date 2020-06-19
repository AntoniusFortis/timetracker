/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Models.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [StringLength( 20 )]
        [Required]
        public string Login { get; set; }

        [Required]
        [JsonIgnore]
        [Column("PassId")]
        public virtual Pass Pass { get; set; }

        [StringLength( 50 )]
        [Required]
        [JsonIgnore]
        public string IV { get; set; }

        [StringLength( 128 )]
        [Required]
        public string FirstName { get; set; }

        [StringLength( 128 )]
        [Required]
        public string Surname { get; set; }

        [StringLength( 128 )]
        public string MiddleName { get; set; }

        [StringLength( 128 )]
        public string BirthDate { get; set; }

        [StringLength( 128 )]
        public string City { get; set; }

        [StringLength( 512 )]
        [Required]
        public string Email { get; set; }

        public int? TokenId { get; set; }
    }
}
