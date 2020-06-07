using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Timetracker.Entities.Models
{
    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }
        
        [StringLength(20)]
        [Required]
        public string Login { get; set; }

        [StringLength(200)]
        [Required]
        public string Pass { get; set; }

        [JsonIgnore]
        [Required]
        public byte[] Salt { get; set; }

        [StringLength(30)]
        [Required]
        public string FirstName { get; set; }

        [StringLength(30)]
        [Required]
        public string Surname { get; set; }

        [StringLength(30)]
        public string MiddleName { get; set; }

        [StringLength( 30 )]
        public string BirthDate { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(255)]
        [Required]
        public string Email { get; set; }

        public int? TokenId { get; set; }
    }
}
