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
                await Clients.Caller.SendAsync( "getActiveTracking", false, null, false, "У вас уже есть задача, которая отслеживается." )
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
                await Clients.Caller.SendAsync( "getActiveTracking", false, null, false, "У вас недостаточно прав, чтобы отслеживать эту задачу." )
                    .ConfigureAwait( false );
                return;
            }

            var now = DateTime.UtcNow;
            var worktrack = await _dbContext.Worktracks.AddAsync( new Worktrack
            {
                UserId = dbUser.Id,
                StartedTime = now,
                StoppedTime = now,
                TaskId = taskId,
                Draft = true
            } )
                .ConfigureAwait( false );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            var group = Clients.Group( userName );

            await group.SendAsync( "getActiveTracking", true, worktrack.Entity, true, string.Empty )
                .ConfigureAwait( false );
        }

        public async Task StopTracking()
        {
            var userName = Context.User.Identity.Name;

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var dbWorktrack = await _dbContext.Worktracks.SingleOrDefaultAsync( x => x.UserId == dbUser.Id && x.Draft )
                .ConfigureAwait(false);

            dbWorktrack.Draft = false;
            dbWorktrack.StoppedTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            var trackedTime = dbWorktrack.StoppedTime - dbWorktrack.StartedTime;
            var formatTime = trackedTime.ToString(@"hh\:mm\:ss");

            await Clients.Group( userName ).SendAsync( "getActiveTracking", false, dbWorktrack, false, "Вы перестали отслеживать задачу. С момента начала прошло: " + formatTime )
                .ConfigureAwait( false );
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.Identity.Name;

            await Groups.AddToGroupAsync( Context.ConnectionId, userName )
                .ConfigureAwait( false );

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var dbWorktrack = await _dbContext.Worktracks.FirstOrDefaultAsync(x => x.UserId == dbUser.Id && x.Draft)
                .ConfigureAwait(false);

            await base.OnConnectedAsync()
                .ConfigureAwait( false );

            await Clients.Caller.SendAsync( "getActiveTracking", dbWorktrack != null, dbWorktrack, false, string.Empty )
                .ConfigureAwait( false );
        }

        public override async Task OnDisconnectedAsync( Exception exception )
        {
            var userName = Context.User.Identity.Name;

            await Clients.Caller.SendAsync( "getActiveTracking", false, null, false, string.Empty )
                .ConfigureAwait(false);

            await base.OnDisconnectedAsync( exception )
                .ConfigureAwait(false);

            await Groups.RemoveFromGroupAsync( Context.ConnectionId, userName );
        }
    }
}
