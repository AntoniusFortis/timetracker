using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;

namespace Timetracker.View.Hubs
{
    [Authorize]
    public class TrackingHub : Hub
    {
        private readonly TimetrackerContext _dbContext;

        public TrackingHub( TimetrackerContext dbContext )
        {
            _dbContext = dbContext;
        }

        public async Task GetActiveTracking()
        {
            var userName = Context.User.Identity.Name;

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var dbWorktrack = await _dbContext.Worktracks
                .FirstOrDefaultAsync(x => x.UserId == dbUser.Id);

            var trackedTime = DateTime.UtcNow - dbWorktrack.StartedTime;
            var formatTime = trackedTime.ToString(@"hh\:mm\:ss");

            await this.Clients.Caller.SendAsync( "getActiveTracking", dbWorktrack != null, dbWorktrack, formatTime );
        }

        public async Task StartTracking( int taskId )
        {
            var userName = Context.User.Identity.Name;

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var anyActiveWorktrack = await _dbContext.Worktracks.AsNoTracking()
                .AnyAsync( x => x.UserId == dbUser.Id && x.Draft )
                .ConfigureAwait(false);

            if ( anyActiveWorktrack )
            {
                await this.Clients.Caller.SendAsync( "startTracking", "У вас уже есть задача, которая отслеживается.", HttpStatusCode.InternalServerError, null )
                    .ConfigureAwait( false );

                return;
            }

            var dbWorktask = await _dbContext.Tasks.AsNoTracking()
                .Select( x => new
                {
                    x.Id,
                    x.ProjectId
                })
                .SingleOrDefaultAsync(x => x.Id == taskId)
                .ConfigureAwait(false);

            if ( !_dbContext.CheckAccessForProject( dbWorktask.ProjectId, dbUser ) )
            {
                await this.Clients.Caller.SendAsync( "startTracking", "У вас недостаточно прав, чтобы отслеживать эту задачу.", HttpStatusCode.Unauthorized, null )
                    .ConfigureAwait( false );

                return;
            }

            var now = DateTime.UtcNow;
            var worktrack = await _dbContext.AddAsync( new Worktrack
            {
                UserId = dbUser.Id,
                StartedTime = now,
                StoppedTime = now,
                TaskId = dbWorktask.Id,
                Draft = true
            } )
                .ConfigureAwait( false );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            await this.Clients.Group( userName ).SendAsync( "startTracking", string.Empty, HttpStatusCode.OK, worktrack )
                .ConfigureAwait( false );
        }

        public async Task StopTracking()
        {
            var userName = Context.User.Identity.Name;

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var dbWorktrack = await _dbContext.Worktracks.Where(x => x.UserId == dbUser.Id)
                .SingleAsync(x => x.Draft)
                .ConfigureAwait(false);

            dbWorktrack.Draft = false;
            dbWorktrack.StoppedTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            var trackedTime = dbWorktrack.StoppedTime - dbWorktrack.StartedTime;
            var formatTime = trackedTime.ToString(@"hh\:mm\:ss");

            await Clients.Group( userName ).SendAsync( "stopTracking", "Вы перестали отслеживать задачу. С момента начало прошло: " + formatTime, HttpStatusCode.OK );
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync().ConfigureAwait(false);

            var userName = Context.User.Identity.Name;

            await Groups.AddToGroupAsync( Context.ConnectionId, userName )
                .ConfigureAwait(false);

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var dbWorktrack = await _dbContext.Worktracks.FirstOrDefaultAsync(x => x.UserId == dbUser.Id && x.Draft);

            if ( dbWorktrack != null )
            {
                var trackedTime = DateTime.UtcNow - dbWorktrack.StartedTime;
                var formatTime = trackedTime.ToString(@"hh\:mm\:ss");

                this.Clients.Caller.SendAsync( "getActiveTracking", true, dbWorktrack, formatTime ).Wait();
            }
            else
            {
                this.Clients.Caller.SendAsync( "getActiveTracking", false, null, null ).Wait();
            }
        }

        public override async Task OnDisconnectedAsync( Exception exception )
        {
            var userName = Context.User.Identity.Name;

            await Groups.RemoveFromGroupAsync( Context.ConnectionId, userName ).ConfigureAwait( false );

            this.Clients.Caller.SendAsync( "getActiveTracking", false, null, null ).Wait();

            await base.OnDisconnectedAsync( exception );
        }
    }
}
