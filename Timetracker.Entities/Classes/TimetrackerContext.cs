using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
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

        private readonly IMemoryCache _cache;

        public TimetrackerContext(DbContextOptions options, IMemoryCache cache) : base(options)
        {
            _cache = cache;
        }
        
        public async Task<User> GetUser(string name)
        {
            if (!_cache.TryGetValue(name, out User user))
            {
                user = await Users.AsNoTracking().FirstOrDefaultAsync(p => p.Name == name);
                if (user != null)
                {
                    _cache.Set(user.Name, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1)));
                }
            }

            return user;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
