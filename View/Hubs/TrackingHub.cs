using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Timetracker.Models.Classes;
using Timetracker.Models.Entities;

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

        public async Task StartTracking( int? taskId )
        {
            var worktaskId = taskId.Value;

            var userName = Context.User.Identity.Name;
            int userId = int.Parse( Context.User.Identity.Name );

            var anyActiveWorktrack = await _dbContext.Worktracks.AsNoTracking()
                    .AnyAsync( x => x.User.Id == userId && x.Running )
                    .ConfigureAwait(false);

            if ( anyActiveWorktrack )
            {
                await Clients.Caller.SendAsync( "getActiveTracking", false, null, false, "У вас уже есть задача, которая отслеживается." )
                    .ConfigureAwait( false );
                return;
            }

            var worktaskData = await _dbContext.Worktasks.AsNoTracking()
                    .Where( x => x.Id == worktaskId )
                    .Select( x => new { x.ProjectId, x.State } )
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

            if ( worktaskData == null )
            {
                await Clients.Caller.SendAsync( "getActiveTracking", false, null, false, "Не существует задачи с таким идентификатором" )
                    .ConfigureAwait( false );
                return;
            }

            var projectId = worktaskData.ProjectId;
            if ( _dbContext.GetLinkedAcceptedProject( projectId, userId ) == null )
            {
                await Clients.Caller.SendAsync( "getActiveTracking", false, null, false, "У вас недостаточно прав, чтобы отслеживать эту задачу." )
                    .ConfigureAwait( false );
                return;
            }

            if ( worktaskData.State.Id == 6 )
            {
                await Clients.Caller.SendAsync( "getActiveTracking", false, null, false, "Данная задача уже закрыта" )
               .ConfigureAwait( false );
                return;
            }

            var now = DateTime.UtcNow;
            var worktrack = await _dbContext.AddAsync( new Worktrack
            {
                UserId = userId,
                StartedTime = now,
                StoppedTime = now,
                WorktaskId = worktaskId,
                Running = true
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
            int userId = int.Parse( Context.User.Identity.Name );

            var dbWorktrack = await _dbContext.Worktracks.FirstOrDefaultAsync( x => x.User.Id == userId && x.Running )
                .ConfigureAwait( false );

            dbWorktrack.Running = false;
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

            int userId = int.Parse( Context.User.Identity.Name );

            // Получить запущенный трек 
            var dbWorktrack = await _dbContext.Worktracks.FirstOrDefaultAsync( x => x.User.Id == userId && x.Running )
                .ConfigureAwait( false );

            await base.OnConnectedAsync()
                .ConfigureAwait( false );

            await Clients.Caller.SendAsync( "getActiveTracking", dbWorktrack != null, dbWorktrack, false, string.Empty )
                .ConfigureAwait( false );
        }

        public override async Task OnDisconnectedAsync( Exception exception )
        {
            var userName = Context.User.Identity.Name;

            // Сообщаем подключению, что соединение разорвано
            await Clients.Caller.SendAsync( "getActiveTracking", false, null, false, string.Empty )
                .ConfigureAwait( false );

            await base.OnDisconnectedAsync( exception )
                .ConfigureAwait( false );

            // Удалить подключение из группы пользователя
            await Groups.RemoveFromGroupAsync( Context.ConnectionId, userName )
                .ConfigureAwait( false );
        }
    }
}
