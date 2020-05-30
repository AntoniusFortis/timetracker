using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
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
        private readonly IMemoryCache _cache;

        public TrackingHub( TimetrackerContext dbContext, IMemoryCache cache )
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        //public async Task GetActiveTracking()
        //{
        //    var userName = Context.User.Identity.Name;

        //    var dbUser = await _dbContext.GetUserAsync(userName)
        //        .ConfigureAwait(false);

        //    var dbWorktrack = await _dbContext.Worktracks
        //        .FirstOrDefaultAsync(x => x.UserId == dbUser.Id);

        //    var trackedTime = DateTime.UtcNow - dbWorktrack.StartedTime;
        //    var formatTime = trackedTime.ToString(@"hh\:mm\:ss");

        //    await this.Clients.Caller.SendAsync( "getActiveTracking", dbWorktrack != null, dbWorktrack, formatTime );
        //}

        public async Task StartTracking( int taskId )
        {
            var rawProjectId = this.Context.GetHttpContext().Request.Query["projectId"].ToString();
            var projectId = int.Parse( rawProjectId );

            var userName = Context.User.Identity.Name;

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var anyActiveWorktrack = await _dbContext.Worktracks.AnyAsync( x => x.UserId == dbUser.Id && x.Task.ProjectId == projectId && x.Draft )
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

            if ( !_dbContext.CheckAccessForProject( dbWorktask.ProjectId, dbUser ) || projectId != dbWorktask.ProjectId )
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

            await this.Clients.Group( userName + projectId ).SendAsync( "startTracking", string.Empty, HttpStatusCode.OK, worktrack )
                .ConfigureAwait( false );
        }

        public async Task StopTracking()
        {
            var rawProjectId = this.Context.GetHttpContext().Request.Query["projectId"].ToString();
            var projectId = int.Parse( rawProjectId );

            var userName = Context.User.Identity.Name;

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var dbWorktrack = await _dbContext.Worktracks.SingleOrDefaultAsync(x => x.UserId == dbUser.Id && x.Task.ProjectId == projectId && x.Draft)
                .ConfigureAwait(false);

            dbWorktrack.Draft = false;
            dbWorktrack.StoppedTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            var trackedTime = dbWorktrack.StoppedTime - dbWorktrack.StartedTime;
            var formatTime = trackedTime.ToString(@"hh\:mm\:ss");

            await Clients.Group( userName + projectId ).SendAsync( "stopTracking", "Вы перестали отслеживать задачу. С момента начало прошло: " + formatTime, HttpStatusCode.OK );
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync().ConfigureAwait( false );

            var userName = Context.User.Identity.Name;

            var projectId = this.Context.GetHttpContext().Request.Query["projectId"].ToString();
            var parsedProjectId = int.Parse( projectId );

            await Groups.AddToGroupAsync( Context.ConnectionId, userName + projectId )
                .ConfigureAwait( false );

            var dbUser = await _dbContext.GetUserAsync(userName)
                .ConfigureAwait(false);

            var dbWorktrack = await _dbContext.Worktracks.FirstOrDefaultAsync(x => x.UserId == dbUser.Id && x.Task.ProjectId == parsedProjectId && x.Draft)
                .ConfigureAwait( false );

            this.Clients.Caller.SendAsync( "getActiveTracking", dbWorktrack != null, dbWorktrack ).Wait();
        }

        public override async Task OnDisconnectedAsync( Exception exception )
        {
            var userName = Context.User.Identity.Name;
            var projectId = this.Context.GetHttpContext().Request.Query["projectId"].ToString();

            await Groups.RemoveFromGroupAsync( Context.ConnectionId, userName + projectId ).ConfigureAwait( false );

            await this.Clients.Caller.SendAsync( "getActiveTracking", false, null );

            await base.OnDisconnectedAsync( exception );
        }
    }
}
