using System.ComponentModel.DataAnnotations;
namespace Timetracker.Models.Models
{
    public class SignInModel
    {
        public string login { get; set; }

        public string pass { get; set; }
    }
}
