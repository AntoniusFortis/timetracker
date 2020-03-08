using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class User
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Имя пользователя")]
        public string Name { get; set; }
    }
}
