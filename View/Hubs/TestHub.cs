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
    public class ChatHub : Hub
    {
        private readonly TimetrackerContext _dbContext;

        public ChatHub(TimetrackerContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendToAll(string str_taskId)
        {
            var user = Context.User;
            var userName = user.Identity.Name;

            var taskId = int.Parse(str_taskId);

            var dbUser = await _dbContext.GetUserAsync(userName);

            if (await _dbContext.Worktracks.AnyAsync(x => x.UserId == dbUser.Id && x.Draft))
            {
                await this.Clients.All.SendAsync("SendToAll", "Вы уже что-то тречите.", HttpStatusCode.InternalServerError);
                return;
            }

            var dbWorktask = await _dbContext.Tasks.SingleAsync(x => x.Id == taskId);

            if (!_dbContext.CheckAccessForProject(dbWorktask.ProjectId, dbUser))
            {
                await this.Clients.All.SendAsync("SendToAll", "У вас нет прав на это дело.", HttpStatusCode.Unauthorized);
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

            await this.Clients.All.SendAsync("SendToAll", "Трекинг успешно начат", HttpStatusCode.OK);
        }

        public async Task StopTracking(string str_taskId)
        {
            var user = Context.User;
            var userName = user.Identity.Name;

            var taskId = int.Parse(str_taskId);

            var dbUser = await _dbContext.GetUserAsync(userName);
            var dbWorktask = await _dbContext.Tasks.SingleAsync(x => x.Id == taskId);

            if (!_dbContext.CheckAccessForProject(dbWorktask.ProjectId, dbUser))
            {
                await this.Clients.All.SendAsync("SendToAll", "У вас нет прав на это дело.", HttpStatusCode.Unauthorized);
                return;
            }

            var dbWorktrack = await _dbContext.Worktracks
                .Where(x => x.TaskId == dbWorktask.Id && x.UserId == dbUser.Id)
                .SingleAsync(x => x.Draft);

            dbWorktrack.Draft = false;
            dbWorktrack.StoppedTime = DateTime.UtcNow;
            var trackedTime = dbWorktrack.StoppedTime - dbWorktrack.StartedTime;
            await _dbContext.SaveChangesAsync();

            await this.Clients.All.SendAsync("StopTracking", "It has been stopped. Your time (min): " + trackedTime.TotalMinutes, HttpStatusCode.OK);
        }
    }
}
