/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Models.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    public class LinkedProject
    {
        [Key]
        public int Id { get; set; }

        public byte RoleId { get; set; }

        public Role Role { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int ProjectId { get; set; }

        [JsonIgnore]
        public Project Project { get; set; }

        public bool Accepted { get; set; }
    }
}
