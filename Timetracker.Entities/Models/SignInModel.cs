using System.ComponentModel.DataAnnotations;
namespace Timetracker.Models.Models
{
    public class SignInModel
    {
        [Required]
        public string login { get; set; }

        [Required]
        public string pass { get; set; }
    }
}
