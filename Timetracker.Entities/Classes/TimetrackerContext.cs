using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timetracker.Entities.Entity;
using Timetracker.Entities.Models;

namespace Timetracker.Entities.Classes
{
    public class TimetrackerContext : DbContext, IDisposable
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Rights { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<AuthorizedUser> AuthorizedUsers { get; set; }
        public DbSet<WorkTask> Worktasks { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Worktrack> Worktracks { get; set; }
        public DbSet<Token> Tokens { get; set; }

        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions cacheOptions;

        public TimetrackerContext(DbContextOptions<TimetrackerContext> options, IMemoryCache cache) : base(options)
        {
            _cache = cache;
            cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));

            Database.Migrate();

            Rights.Load();
            States.Load();
            Tokens.Load();
            Users.Load();
            Projects.Load();
            AuthorizedUsers.Load();
            Worktasks.Load();
            Worktracks.Load();
        }

        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
            base.OnModelCreating( modelBuilder );

            modelBuilder.Entity<Worktrack>()
            .HasOne( p => p.Worktask )
            .WithMany( t => t.WorkTracks )
            .OnDelete( DeleteBehavior.Cascade );

            modelBuilder.Entity<WorkTask>()
                .HasOne( p => p.Project )
                .WithMany( t => t.Tasks )
                .OnDelete( DeleteBehavior.Cascade );
        }

        public async Task<bool> UserExists( string name )
        {
            return await Users.AsNoTracking().AnyAsync( p => p.Login == name );
        }

        public async Task<User> GetUserAsync( string name, bool invalidateCache = false )
        {
            if ( invalidateCache || !_cache.TryGetValue( $"UserObject:{name}", out User user ) )
            {
                user = await Users
                    .SingleOrDefaultAsync( p => p.Login == name )
                    .ConfigureAwait( false );

                if ( user != null )
                {
                    _cache.Set( $"UserObject:{user.Login}", user, cacheOptions );
                }
            }

            return user;
        }

        public AuthorizedUser GetLinkedProjectForUser( int projectId, int userId )
        {
            return AuthorizedUsers.FirstOrDefault( x => x.ProjectId == projectId && x.UserId == userId );
        }

        public bool CheckAccessForProject( int projectId, User user, bool invalidateCache = false )
        {
            var key = $"Access:{projectId}{user.Id}";
            if ( !_cache.TryGetValue( key, out bool hasAU ) || invalidateCache )
            {
                hasAU = AuthorizedUsers.AsNoTracking()
                    .Any( x => x.ProjectId == projectId && x.User.Id == user.Id && x.Accepted );

                if ( hasAU )
                {
                    _cache.Set( key, hasAU, cacheOptions );
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
