using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Models;

namespace Timetracker.Entities.Classes
{
    public class AuthorizedUsersRepository : BaseRepository<AuthorizedUser>
    {
        public override DbSet<AuthorizedUser> Context => _contextDB.AuthorizedUsers;

        public AuthorizedUsersRepository(TimetrackerContext context)
        {
            _contextDB = context;
        }
    }
}
