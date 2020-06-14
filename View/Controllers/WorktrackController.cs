using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;
using Timetracker.Models.Response;
using Timetracker.Models.Responses;

namespace Timetracker.Entities.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class WorktrackController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public WorktrackController(TimetrackerContext dbContext)
        {
            _dbContext = dbContext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public async Task<JsonResult> GetAll( [FromQuery] uint? id )
        {
            if ( !id.HasValue )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Вы не предоставили поле id"
                } );
            }

            if ( !int.TryParse( id.Value.ToString(), out var taskId ) )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Недопустимый формат идентификатора"
                } );
            }

            var worktracks = await _dbContext.Worktracks.Where(x => x.WorktaskId == taskId && !x.Running )
                .OrderByDescending( x => x.StartedTime )
                .Select( x => new {
                    id = x.Id,
                    login = x.User.Login,
                    startedTime = x.StartedTime,
                    stoppedTime = x.StoppedTime,
                    totalTime = (x.StoppedTime - x.StartedTime).ToString(@"hh\:mm\:ss"),
                    projectId = x.Worktask.ProjectId
                })
                .ToListAsync()
                .ConfigureAwait(false);

            if ( worktracks.Count == 0 )
            {
                return new JsonResult( new List<WorktracksGetAllResponse>() );
            }

            var projectId = worktracks.Select( x => x.projectId ).FirstOrDefault();
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var linkedProject = _dbContext.GetLinkedProjectForUser( projectId, user.Id );
            if ( linkedProject == null )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "У вас нет доступа к данному проекту"
                } );
            }

            return new JsonResult( worktracks.Select( x => new WorktracksGetAllResponse
            {
                id = x.id,
                login = x.login,
                startedTime = x.startedTime,
                stoppedTime = x.stoppedTime,
                totalTime = x.totalTime
            } ).ToList() );
        }

        [HttpPost]
        public async Task<JsonResult> GetReport( [FromBody] ReportModel model )
        {
            var startedDate = DateTime.Parse( model.startDate );
            var endDate = DateTime.Parse( model.endDate ).AddDays(1);

            if ( endDate < startedDate )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Конечная дата не может быть меньше начальной"
                } );
            }

            var worktracksQuery =  _dbContext.Worktracks.Where( x => x.Worktask.ProjectId == model.projectId && x.StartedTime >= startedDate && x.StoppedTime <= endDate );

            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var linkedProject = _dbContext.GetLinkedProjectForUser( model.projectId, user.Id );
            if ( linkedProject == null )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "У вас нет доступа к данному проекту"
                } );
            }

            // Если у пользователя роль Пользователь
            if ( linkedProject.RightId == 1 )
            {
                worktracksQuery = worktracksQuery.Where( x => x.UserId == user.Id );
            }

            if ( model.userId.HasValue && model.userId != 0 )
            {
                worktracksQuery = worktracksQuery.Where( x => x.UserId == model.userId.Value );
            }

            if ( model.taskId.HasValue && model.taskId != 0 )
            {
                worktracksQuery = worktracksQuery.Where( x => x.WorktaskId == model.taskId.Value );
            }

            var worktracks = await worktracksQuery.Select( x => new ReportResponse
            {
                id = x.Id,
                login = x.User.Login,
                task = x.Worktask.Title,
                taskId = x.WorktaskId,
                startedTime = x.StartedTime,
                stoppedTime = x.StoppedTime,
                totalTime = (x.StoppedTime - x.StartedTime).ToString(@"hh\:mm\:ss")
            })
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult( worktracks );
        }
    }
}
