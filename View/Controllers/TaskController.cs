/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Entities.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using Timetracker.Models.Classes;
    using Timetracker.Models.Entities;
    using Timetracker.Models.Extensions;
    using Timetracker.Models.Models;
    using Timetracker.Models.Responses;
    using Timetracker.View.Hubs;
    using Timetracker.View.Resources;

    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status403Forbidden )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status400BadRequest )]
    public class TaskController : ControllerBase
    {
        private readonly TimetrackerContext _context;
        private readonly Lazy<IHubContext<TrackingHub>> _hub;

        public TaskController(TimetrackerContext context, IHubContext<TrackingHub> hub )
        {
            _context = context;
            _hub = new Lazy<IHubContext<TrackingHub>>( hub );
        }

        /// <summary>
        /// Получить информацию о задаче
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>   
        /// <returns>Информация о задаче</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>   
        [HttpGet]
        [ProducesResponseType( typeof( WorktaskGetResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Get( [FromQuery] uint? id )
        {
            int taskId = this.ParseValue( id );

            var worktask = await _context.Worktasks.FirstOrDefaultAsync( x => x.Id == taskId )
                .ConfigureAwait(false);
            if ( worktask == null )
            {
                throw new Exception( TextResource.API_NotExistWorktaskId );
            }

            int projectId = worktask.ProjectId;
            int userId = int.Parse( User.Identity.Name );

            // Проверяем доступ
            var linkedProject = await _context.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            var task = new
            {
                worktask.Id,
                worktask.ProjectId,
                worktask.StateId,
                worktask.Title,
                worktask.Description,
                worktask.Duration,
                worktask.CreatedDate
            };

            return new JsonResult( new WorktaskGetResponse
            {
                project = worktask.Project,
                worktask = task,
                isAdmin = linkedProject.RoleId
            }, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            } );
        }

        /// <summary>
        /// Добавить задачу
        /// </summary>
        /// <param name="model">Данные для добавления задачи</param>  
        /// <returns>Информация о добавленной задаче</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>  
        [HttpPost]
        [ProducesResponseType( typeof( WorktaskAddResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Add( [FromBody] WorktaskUpdateModel model )
        {
            var worktask = model.worktask;

            int projectId = worktask.ProjectId;
            int userId = int.Parse( User.Identity.Name );

            // Проверяем доступ
            var linkedProject = await _context.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            worktask.CreatedDate = DateTime.UtcNow;

            var addedTask = await _context.AddAsync(worktask)
                .ConfigureAwait( false );

            await _context.SaveChangesAsync( true )
                .ConfigureAwait( false );

            return new JsonResult( new WorktaskAddResponse { worktask = worktask } );
        }

        /// <summary>
        /// Изменить задачу
        /// </summary>
        /// <param name="model">Данные для изменения задачи</param>  
        /// <returns>Информация об изменённой задаче</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>  
        [HttpPost]
        [ProducesResponseType( typeof( WorktaskUpdateResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Update( [FromBody] WorktaskUpdateModel model )
        {
            var worktask = model.worktask;

            int projectId = worktask.ProjectId;
            int userId = int.Parse( User.Identity.Name );

            // Проверяем доступ
            var linkedProject = await _context.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null || linkedProject.RoleId == 1 )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            var dbWorkTask = await _context.Worktasks.FirstOrDefaultAsync( x => x.Id == worktask.Id )
                .ConfigureAwait( false );

            dbWorkTask.Title = worktask.Title;
            dbWorkTask.Description = worktask.Description;
            dbWorkTask.Duration = worktask.Duration;
            dbWorkTask.StateId = worktask.StateId;

            _context.Update( dbWorkTask );

            await _context.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new WorktaskUpdateResponse { worktask = worktask } );
        }

        /// <summary>
        /// Удалить задачу
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>  
        /// <returns>Информация об удалённой задаче</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response> 
        [HttpDelete]
        [ProducesResponseType( typeof( WorktaskDeleteResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Delete( [FromQuery] uint? id )
        {
            int taskId = this.ParseValue( id );

            var worktask = await _context.Worktasks.FirstOrDefaultAsync( x => x.Id == taskId )
                .ConfigureAwait(false);
            if ( worktask == null )
            {
                throw new Exception( TextResource.API_NotExistWorktaskId );
            }

            int projectId = worktask.ProjectId;
            int userId = int.Parse( User.Identity.Name );

            // Проверяем доступ
            var linkedProject = await _context.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null || linkedProject.RoleId == 1 )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            // Если на задачу кто-то тречит - заставить их прекратить
            var runningWorktracks = _context.Worktracks.Where( x => x.WorktaskId == taskId && x.Running );
            if ( runningWorktracks.Any() )
            {
                var users = runningWorktracks.Select( x => x.UserId );

                foreach ( var userName in users )
                {
                    await _hub.Value.Clients.Group( userName.ToString() ).SendAsync( "getActiveTracking", false, null, false, TextResource.SignalR_TaskIsRemoved )
                        .ConfigureAwait( false );
                }
            }

            _context.Remove( worktask );

            await _context.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new WorktaskDeleteResponse { worktask = worktask } );
        }
    }
}