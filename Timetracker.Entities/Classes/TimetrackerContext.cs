using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timetracker.Entities.Models;

namespace Timetracker.Entities.Classes
{
    public class TimetrackerContext : DbContext, IDisposable
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Right> Rights { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<AuthorizedUser> AuthorizedUsers { get; set; }
        public DbSet<WorkTask> Tasks { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Worktrack> Worktracks { get; set; }

        private readonly IMemoryCache _cache;

        public TimetrackerContext(DbContextOptions options, IMemoryCache cache) : base(options)
        {
            _cache = cache;

            Users.Load();
            Projects.Load();
            AuthorizedUsers.Load();
            Tasks.Load();
            States.Load();
            Worktracks.Load();
        }

        public bool UserExist(string name)
        {
            var exist = Users.AsNoTracking().Any(p => p.Login.Contains(name));

            return exist;
        }

        public async Task<User> GetUserAsync(string name)
        {
            if (!_cache.TryGetValue(name, out User user))
            {
                user = await Users.AsNoTracking().FirstOrDefaultAsync(p => p.Login == name);
                if (user != null)
                {
                    _cache.Set(user.Login, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1)));
                }
            }

            return user;
        }

        public bool CheckAccessForProject(int projectId, User user)
        {
            var hasAU = AuthorizedUsers.AsNoTracking()
                .Any(x => x.ProjectId == projectId && x.User.Id == user.Id);

            return hasAU;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
