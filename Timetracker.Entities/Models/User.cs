using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class User
    {
        public int Id { get; set; }
 
        public string Login { get; set; }

        public string Pass { get; set; }

        public byte[] Salt { get; set; }
    }
}
