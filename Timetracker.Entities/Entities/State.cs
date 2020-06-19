/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Models.Entities
{
    using System.ComponentModel.DataAnnotations;

    public class State
    {
        [Key]
        public byte Id { get; set; }

        [Required]
        [StringLength( 60 )]
        public string Title { get; set; }
    }
}