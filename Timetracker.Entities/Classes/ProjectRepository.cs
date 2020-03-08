using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Models;

namespace Timetracker.Entities.Classes
{
    public class ProjectRepository : BaseRepository<Project>
    {
        public override DbSet<Project> Context => _contextDB.Projects;

        public ProjectRepository(TimetrackerContext context)
        {
            _contextDB = context;
        }
    }
}
