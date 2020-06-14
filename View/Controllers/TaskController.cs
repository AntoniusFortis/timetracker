using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Hubs;
using Timetracker.Entities.Models;
using Timetracker.Models.Models;
using Timetracker.Models.Response;
using Timetracker.Models.Responses;

namespace Timetracker.Entities.Controllers
{
    public class UpdateStateModel
    {
        public int TaskId { get; set; }

        public int StateId { get; set; }
    }

    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class TaskController : Controller
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHubContext<TrackingHub> _hub;

        public TaskController(TimetrackerContext dbContext, IHubContext<TrackingHub> hub )
        {
            _dbContext = dbContext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _hub = hub;
        }

        [HttpPost]
        public async Task<JsonResult> Add( [FromBody] WorktaskUpdateModel model )
        {
            var worktask = model.worktask;
            var user = await _dbContext.GetUserAsync(User.Identity.Name).
                ConfigureAwait(false);

            if (!_dbContext.CheckAccessForProject(worktask.ProjectId, user))
            {
                return new JsonResult(new ErrorResponse
                {
                    message = "У вас нет прав для добавления новой задачи"
                } );
            }

            worktask.CreatedDate = DateTime.UtcNow;

            var addedTask = await _dbContext.AddAsync(worktask)
                .ConfigureAwait( false );

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new OkResponse
            {
                message = "Вы успешно добавили задачу"
            } );
        }

        [HttpGet]
        public async Task<JsonResult> Get(int? Id)
        {
            if ( !Id.HasValue )
            {
                return new JsonResult( new
                {
                    status = HttpStatusCode.BadRequest,
                    message = "Вы должны ввести идентификатор"
                } );
            }

            var id = Id.Value;

            var worktask = await _dbContext.Worktasks.SingleOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var linkedProject = _dbContext.GetLinkedProjectForUser( worktask.ProjectId, user.Id );
            if ( linkedProject == null || !linkedProject.Accepted )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "У вас нет доступа для изменения задачи"
                } );
            }

            var task = new
            {
                worktask.Id,
                worktask.ProjectId,
                worktask.StateId,
                worktask.Title,
                worktask.Description,
                worktask.Duration,
                Project = new { worktask.Project.Id, worktask.Project.Title },
                worktask.CreatedDate
            };

            return new JsonResult(new
            {
                status = HttpStatusCode.OK,
                project = worktask.Project,
                worktask = task,
                isAdmin = linkedProject.RightId
            }, _jsonOptions);
        }


        [HttpPost]
        public async Task<JsonResult> UpdateState( UpdateStateModel model )
        {
            var dbWorkTask = _dbContext.Worktasks.FirstOrDefault(x => x.Id == model.TaskId );

            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait( false );
            var linkedProject = _dbContext.GetLinkedProjectForUser( dbWorkTask.ProjectId, user.Id );

            if ( linkedProject == null )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "У вас нет доступа для изменения задачи"
                } );
            }

            dbWorkTask.StateId = ( byte ) model.StateId;
            _dbContext.Update( dbWorkTask );

            await _dbContext.SaveChangesAsync();

            return new JsonResult( new
            {
                status = HttpStatusCode.OK
            }, _jsonOptions );
        }

        [HttpPost]
        public async Task<JsonResult> Update( [FromBody] WorktaskUpdateModel model )
        {
            var worktask = model.worktask;
            var user = await _dbContext.GetUserAsync(User.Identity.Name).ConfigureAwait( false );

            var linkedProject = _dbContext.GetLinkedProjectForUser( worktask.Project.Id, user.Id );

            if ( linkedProject == null || linkedProject.RightId == 1 )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "У вас нет доступа для изменения задачи"
                } );
            }

            var dbWorkTask = _dbContext.Worktasks.FirstOrDefault(x => x.Id == model.worktask.Id);
            dbWorkTask.Title = model.worktask.Title;
            dbWorkTask.Description = model.worktask.Description;
            dbWorkTask.Duration = model.worktask.Duration;
            dbWorkTask.StateId = model.worktask.StateId;

            _dbContext.Update( dbWorkTask );

            await _dbContext.SaveChangesAsync();

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                worktask = worktask
            }, _jsonOptions );
        }

        [HttpDelete]
        public async Task<JsonResult> Delete( [FromQuery] uint? id)
        {
            if ( !id.HasValue )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Вы должны ввести идентификатор"
                } );
            }

            var worktaskId = id.Value;
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var dbWorkTask = await _dbContext.Worktasks.FirstOrDefaultAsync(x => x.Id == worktaskId)
                .ConfigureAwait(false);

            var hasAccess = await _dbContext.LinkedProjects.AsNoTracking()
                .AnyAsync( x => x.ProjectId == dbWorkTask.ProjectId && x.User.Id == user.Id && x.RightId != 1 )
                .ConfigureAwait(false);

            if ( !hasAccess )
            {
                return new JsonResult(new ErrorResponse
                {
                    message = "У вас недостаточно прав для удаления задачи"
                } );
            }

            // Если на задачу кто-то тречит - заставить их прекратить
            var runningWorktracks = _dbContext.Worktracks.Where( x => x.WorktaskId == dbWorkTask.Id && x.Running);
            if ( runningWorktracks.Any() )
            {
                var users = runningWorktracks.Select( x => x.User.Login );

                foreach ( var userName in users )
                {
                    await _hub.Clients.Group( userName ).SendAsync( "getActiveTracking", false, null, false, "Задача была удалена и больше не отслеживается" );
                }
            }

            _dbContext.Remove(dbWorkTask);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            return new JsonResult(new WorktaskDeleteResponse
            {
                worktask = dbWorkTask
            }, _jsonOptions);
        }
    }
}