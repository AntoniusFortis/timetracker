
using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Responses
{
    public class SignUpModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Pass { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string Surname { get; set; }

        public string MiddleName { get; set; }

        [Required]
        public string Email { get; set; }

        public string City { get; set; }

        public string BirthDate { get; set; }
    }

    public class MyPageModel
    {
        public string Login { get; set; }

        public string CurrentPass { get; set; }

        public string Pass { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string MiddleName { get; set; }

        public string City { get; set; }

        public string BirthDate { get; set; }

        public string Email { get; set; }
    }
}

