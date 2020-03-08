using Timetracker.Entities.Models;

namespace Timetracker.Models
{
    public class ProjectsView
    {
        public Project[] SignedProjects { get; set; }

        public Project[] NotSignedProjects { get; set; }

    }
}
