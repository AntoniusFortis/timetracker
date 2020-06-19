/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Models.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Token
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength( 50 )]
        public string RefreshToken { get; set; }

        [Required]
        [StringLength( 1024 )]
        public string AccessToken { get; set; }

        [Required]
        public DateTime TokenExpiredDate { get; set; }
    }
}
