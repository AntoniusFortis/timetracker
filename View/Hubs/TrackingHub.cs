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

            var dbUser = await _dbContext.GetUserAsync(userName);

            var dbWorktrack = await _dbContext.Worktracks
                .Where(x => x.UserId == dbUser.Id)
                .AnyAsync(x => x.Draft);

            await this.Clients.Caller.SendAsync("getActiveTracking", dbWorktrack);
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
                await this.Clients.Caller.SendAsync( "startTracking", "У вас уже есть задача, которая отслеживается.", HttpStatusCode.InternalServerError )
                    .ConfigureAwait( false );

                return;
            }

            var dbWorktask = await _dbContext.Tasks.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == taskId)
                .ConfigureAwait(false);

            if ( !_dbContext.CheckAccessForProject( dbWorktask.ProjectId, dbUser ) )
            {
                await this.Clients.Caller.SendAsync( "startTracking", "У вас недостаточно прав, чтобы отслеживать эту задачу.", HttpStatusCode.Unauthorized )
                    .ConfigureAwait( false );

                return;
            }

            await _dbContext.AddAsync( new Worktrack
            {
                UserId = dbUser.Id,
                StartedTime = DateTime.Now,
                StoppedTime = DateTime.Now,
                TaskId = dbWorktask.Id,
                Draft = true
            } )
                .ConfigureAwait( false );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            await this.Clients.Caller.SendAsync( "startTracking", string.Empty, HttpStatusCode.OK )
                .ConfigureAwait( false );
        }

        public async Task StopTracking()
        {
            var userName = Context.User.Identity.Name;

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var dbWorktrack = await _dbContext.Worktracks
                .Where(x => x.UserId == dbUser.Id)
                .SingleAsync(x => x.Draft)
                .ConfigureAwait(false);

            dbWorktrack.Draft = false;
            dbWorktrack.StoppedTime = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            var trackedTime = dbWorktrack.StoppedTime - dbWorktrack.StartedTime;
            var formatTime = trackedTime.ToString(@"hh\:mm\:ss");
            await this.Clients.Caller.SendAsync( "stopTracking", "Вы перестали отслеживать задачу. С момента начало прошло: " + formatTime, HttpStatusCode.OK );
        }

        public override Task OnConnectedAsync()
        {
            base.OnConnectedAsync().Wait();

            var userName = Context.User.Identity.Name;
            var dbUser = _dbContext.GetUserAsync(userName).Result;
            var dbWorktrack = _dbContext.Worktracks.Any(x => x.UserId == dbUser.Id && x.Draft);

            this.Clients.Caller.SendAsync( "getActiveTracking", dbWorktrack ).Wait();

            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            this.Clients.Caller.SendAsync("getActiveTracking", false).Wait();

            return base.OnDisconnectedAsync(exception);
        }
    }
}
