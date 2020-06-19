/* Автор: Антон Другалев  
 * Проект: Timetracker.View
 */

namespace Timetracker.View.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Timetracker.Models.Classes;
    using Timetracker.Models.Extensions;
    using Timetracker.Models.Models;
    using Timetracker.Models.Responses;
    using Timetracker.View.Resources;

    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status403Forbidden )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status400BadRequest )]
    public class WorktrackController : ControllerBase
    {
        private readonly TimetrackerContext _context;

        public WorktrackController( TimetrackerContext context )
        {
            _context = context;
        }

        /// <summary>
        /// Получить все треки задачи
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>   
        /// <returns>Список всех треков задачи</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>   
        [HttpGet]
        [ProducesResponseType( typeof( WorktracksGetAllResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> GetAll( [FromQuery] uint? id )
        {
            int taskId = this.ParseValue( id );

            var task = await _context.Worktasks.FirstOrDefaultAsync( x => x.Id == taskId )
                .ConfigureAwait( false );
            if ( task == null )
            {
                throw new Exception( TextResource.API_NotExistWorktaskId );
            }

            int projectId = task.ProjectId;
            int userId = int.Parse( User.Identity.Name );

            // Проверяем доступ
            var linkedProject = await _context.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            // Собираем информацию по трекам
            var worktracks = await _context.Worktracks.Where(x => x.WorktaskId == taskId && !x.Running )
                .OrderByDescending( x => x.StartedTime )
                .Select( x => new WorktracksGetAllResponse
                {
                    id = x.Id,
                    login = x.User.Login,
                    startedTime = x.StartedTime,
                    stoppedTime = x.StoppedTime,
                    totalTime = ( x.StoppedTime - x.StartedTime ).ToString( @"hh\:mm\:ss" )
                })
                .ToListAsync()
                .ConfigureAwait(false);

            if ( worktracks.Count == 0 )
            {
                return new JsonResult( new List<WorktracksGetAllResponse>() );
            }

            return new JsonResult( worktracks );
        }

        /// <summary>
        /// Получить отчёт
        /// </summary>
        /// <param name="model">Данные для составления отчёта</param>   
        /// <returns>Отчёт с треками</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>   
        [HttpPost]
        [ProducesResponseType( typeof( ReportResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> GetReport( [FromBody] ReportModel model )
        {
            int projectId = model.projectId;
            int userId = int.Parse( User.Identity.Name );

            // Проверяем доступ
            var linkedProject = await _context.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            if ( !DateTime.TryParse( model.startDate, out var startedDate ) )
            {
                throw new Exception( TextResource.API_BadDateFormat );
            }

            if ( !DateTime.TryParse( model.endDate, out var endDate ) )
            {
                throw new Exception( TextResource.API_BadDateFormat );
            }

            endDate = endDate.AddDays( 1 );

            if ( endDate < startedDate )
            {
                throw new Exception( TextResource.API_BadDateCompare );
            }

            bool projectExist = await _context.Projects.AsNoTracking()
                .AnyAsync( x => x.Id == model.projectId )
                .ConfigureAwait( false );
            if ( !projectExist )
            {
                throw new Exception( TextResource.API_NotExistProjectId );
            }

            var worktracksQuery =  _context.Worktracks.Where( x => x.Worktask.ProjectId == model.projectId && x.StartedTime >= startedDate && x.StoppedTime <= endDate );

            // Если у пользователя роль Пользователь
            if ( linkedProject.RoleId == 1 )
            {
                worktracksQuery = worktracksQuery.Where( x => x.UserId == userId );
            }

            // Если вбит пользователь, фильтруем по пользователю
            if ( model.userId.HasValue && model.userId > 0 )
            {
                bool userExist = await _context.Users.AsNoTracking()
                .AnyAsync( x => x.Id == model.userId.Value )
                .ConfigureAwait( false );
                if ( userExist )
                {
                    throw new Exception( TextResource.API_NotExistUserId );
                }

                worktracksQuery = worktracksQuery.Where( x => x.UserId == model.userId.Value );
            }

            // Если вбита задача, фильтруем и по ней
            if ( model.taskId.HasValue && model.taskId > 0 )
            {
                bool taskExist = await _context.Worktasks.AsNoTracking()
                .AnyAsync( x => x.Id == model.taskId.Value )
                .ConfigureAwait( false );
                if ( !taskExist )
                {
                    throw new Exception( TextResource.API_NotExistWorktaskId );
                }

                worktracksQuery = worktracksQuery.Where( x => x.WorktaskId == model.taskId.Value );
            }

            // Собираем информацию по трекам
            var worktracks = await worktracksQuery.Select( x => new ReportResponse
            {
                id = x.Id,
                login = x.User.Login,
                task = x.Worktask.Title,
                taskId = x.WorktaskId,
                startedTime = x.StartedTime,
                stoppedTime = x.StoppedTime,
                totalTime = ( x.StoppedTime - x.StartedTime ).ToString( @"hh\:mm\:ss" )
            })
                .ToListAsync()
                .ConfigureAwait( false );

            return new JsonResult( worktracks );
        }
    }
}
