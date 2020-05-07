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

        public TrackingHub(TimetrackerContext dbContext)
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

        public async Task StartTracking(int taskId)
        {
            var userName = Context.User.Identity.Name;

            var dbUser = await _dbContext.GetUserAsync(userName);

            if (await _dbContext.Worktracks.AnyAsync(x => x.UserId == dbUser.Id && x.Draft))
            {
                await this.Clients.Caller.SendAsync("startTracking", "У вас уже есть задача, которая отслеживается.", HttpStatusCode.InternalServerError);
                return;
            }

            var dbWorktask = await _dbContext.Tasks.SingleAsync(x => x.Id == taskId);
            if (!_dbContext.CheckAccessForProject(dbWorktask.ProjectId, dbUser))
            {
                await this.Clients.Caller.SendAsync("startTracking", "У вас недостаточно прав, чтобы отслеживать эту задачу.", HttpStatusCode.Unauthorized);
                return;
            }

            _dbContext.Add(new Worktrack {
                UserId = dbUser.Id,
                StartedTime = DateTime.UtcNow,
                StoppedTime = DateTime.UtcNow,
                TaskId = dbWorktask.Id,
                Draft = true
            });

            await _dbContext.SaveChangesAsync();

            await this.Clients.Caller.SendAsync("startTracking", String.Empty, HttpStatusCode.OK);
        }

        public async Task StopTracking()
        {
            var userName = Context.User.Identity.Name;

            var dbUser = await _dbContext.GetUserAsync(userName);

            var dbWorktrack = await _dbContext.Worktracks
                .Where(x => x.UserId == dbUser.Id)
                .SingleAsync(x => x.Draft);

            dbWorktrack.Draft = false;
            dbWorktrack.StoppedTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var trackedTime = dbWorktrack.StoppedTime - dbWorktrack.StartedTime;
            var formatTime = trackedTime.ToString("G");
            await this.Clients.Caller.SendAsync("stopTracking", "Вы перестали отслеживать задачу. С момента начало прошло: " + formatTime, HttpStatusCode.OK);
        }

        public override Task OnConnectedAsync()
        {
            base.OnConnectedAsync().Wait();

            var userName = Context.User.Identity.Name;
            var dbUser = _dbContext.GetUserAsync(userName).Result;
            var dbWorktrack = _dbContext.Worktracks.Any(x => x.UserId == dbUser.Id && x.Draft);

            this.Clients.Caller.SendAsync("getActiveTracking", dbWorktrack).Wait();

            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            this.Clients.Caller.SendAsync("getActiveTracking", false).Wait();

            return base.OnDisconnectedAsync(exception);
        }
    }
}
