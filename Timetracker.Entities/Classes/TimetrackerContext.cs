/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Models.Classes
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Timetracker.Models.Entities;

    public class TimetrackerContext : DbContext, IDisposable
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<LinkedProject> LinkedProjects { get; set; }
        public DbSet<WorkTask> Worktasks { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Worktrack> Worktracks { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Pass> Passes { get; set; }

        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions cacheOptions;

        public TimetrackerContext(DbContextOptions<TimetrackerContext> options, IMemoryCache cache) : base(options)
        {
            _cache = cache;
            cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));

            Database.Migrate();

            Roles.Load();
            States.Load();
            Tokens.Load();
            Passes.Load();
            Users.Load();
            Projects.Load();
            LinkedProjects.Load();
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

        public async Task<User> GetUserAsync( string id, bool invalidateCache = false )
        {
            var key = $"UserObject:{id}";
            var userId = int.Parse( id );

            if ( invalidateCache || !_cache.TryGetValue( key, out User user ) )
            {
                user = await Users.FirstOrDefaultAsync( p => p.Id == userId )
                    .ConfigureAwait( false );

                if ( user != null )
                {
                    _cache.Set( key, user, cacheOptions );
                }
            }

            return user;
        }

        public LinkedProject GetLinkedProjectForUser( int projectId, int userId )
        {
            return LinkedProjects.FirstOrDefault( x => x.ProjectId == projectId && x.UserId == userId );
        }

        public async Task<LinkedProject> GetLinkedAcceptedProject( int projectId, int userId, bool invalidateCache = false )
        {
            var key = $"LinkedAcceptedProject:{projectId}:{userId}";
            if ( invalidateCache || !_cache.TryGetValue( key, out LinkedProject linkedProject ) )
            {
                linkedProject = await LinkedProjects.FirstOrDefaultAsync( x => x.ProjectId == projectId && x.UserId == userId && x.Accepted );

                if ( linkedProject != null )
                {
                    _cache.Set( key, linkedProject, cacheOptions );
                }
            }

            return linkedProject;
        }


        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
