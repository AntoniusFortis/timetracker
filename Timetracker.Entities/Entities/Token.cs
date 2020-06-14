using System;
using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Entity
{
    public class Token
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength( 50 )]
        public string RefreshToken { get; set; }

        [Required]
        [StringLength( 305 )]
        public string AccessToken { get; set; }

        public DateTime TokenExpiredDate { get; set; }
    }
}
