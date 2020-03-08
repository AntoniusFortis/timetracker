using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Models;

namespace Timetracker.Entities.Classes
{
    public class UserRepository : BaseRepository<User>
    {
        public override DbSet<User> Context => _contextDB.Users;

        public UserRepository(TimetrackerContext context)
        {
            _contextDB = context;
        }
    }
}
