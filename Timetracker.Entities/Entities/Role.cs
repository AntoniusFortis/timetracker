/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Models.Entities
{
    using System.ComponentModel.DataAnnotations;

    public class Role
    {
        [Key]
        public byte Id { get; set; }

        [Required]
        [StringLength( 50 )]
        public string Title { get; set; }
    }
}
