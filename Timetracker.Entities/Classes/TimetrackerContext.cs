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
        private readonly MemoryCacheEntryOptions cacheOptions;

        public TimetrackerContext(DbContextOptions options, IMemoryCache cache) : base(options)
        {
            _cache = cache;
            cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));

            Database.Migrate();

            Users.Load();
            Projects.Load();
            AuthorizedUsers.Load();
            Tasks.Load();
            States.Load();
            Worktracks.Load();
        }

        public bool UserExists(string name)
        {
            var exist = Users.AsNoTracking().Any(p => p.Login.Contains(name));
            return exist;
        }

        public async Task<User> GetUserAsync(string name, bool invalidateCache = false)
        {
            if ( invalidateCache || !_cache.TryGetValue(name, out User user) )
            {
                user = await Users.AsNoTracking()
                    .SingleOrDefaultAsync(p => p.Login == name)
                    .ConfigureAwait(false);

                if (user != null)
                {
                    _cache.Set(user.Login, user, cacheOptions);
                }
            }

            return user;
        }

        public bool CheckAccessForProject(int projectId, User user)
        {
            var key = $"{projectId}{user.Id}";
            if (!_cache.TryGetValue(key, out bool hasAU))
            {
                hasAU = AuthorizedUsers.AsNoTracking()
                    .Any(x => x.ProjectId == projectId && x.User.Id == user.Id);

                if (hasAU)
                {
                    _cache.Set(key, hasAU, cacheOptions);
                }
            }

            return hasAU;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
