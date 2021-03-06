﻿/* Автор: Антон Другалев  
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
    using Timetracker.Models.Models;
    using Timetracker.Models.Responses;
    using Timetracker.View.Hubs;
    using Timetracker.View.Resources;

    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status403Forbidden )]
    public class StateController : ControllerBase
    {
        private readonly TimetrackerContext _context;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Lazy<IHubContext<TrackingHub>> _hub;

        public StateController(TimetrackerContext dbContext, IHubContext<TrackingHub> hub )
        {
            _context = dbContext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _hub = new Lazy<IHubContext<TrackingHub>>( hub );
        }

        /// <summary>
        /// Получить все доступные состояния задач
        /// </summary>
        /// <returns>Список всех состояний задач</returns>
        [HttpGet]
        [ProducesResponseType( typeof( StateGetAllResponse ), StatusCodes.Status200OK )]
        [ResponseCache( Duration = 360 )]
        public async Task<JsonResult> GetAll()
        {
            var states = await _context.States.AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult( new StateGetAllResponse
            {
                states = states,
            }, _jsonOptions );
        }

        /// <summary>
        /// Изменить состояние задачи
        /// </summary>
        /// <param name="model">Данные для осуществления изменения задачи</param>  
        /// <returns>Сообщение об успешной смене состояния</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата данных</response>  
        [HttpPost]
        [ProducesResponseType( typeof( OkResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Update( [FromBody] UpdateStateModel model )
        {
            var dbWorkTask = await _context.Worktasks.FirstOrDefaultAsync(x => x.Id == model.TaskId )
                .ConfigureAwait( false );
            if ( dbWorkTask == null )
            {
                throw new Exception( TextResource.API_NotExistWorktaskId );
            }

            var stateExist = await _context.States.AnyAsync(x => x.Id == model.StateId )
                .ConfigureAwait( false );
            if ( !stateExist )
            {
                throw new Exception( TextResource.API_NotExistStateId );
            }

            int projectId = dbWorkTask.ProjectId;
            int userId = int.Parse( User.Identity.Name );

            var linkedProject = _context.GetLinkedAcceptedProject( projectId, userId );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            // Если на задачу кто-то тречит - заставить их прекратить
            var runningWorktracks = _context.Worktracks.Where( x => x.WorktaskId == dbWorkTask.Id && x.Running );
            if ( model.StateId == 6 && runningWorktracks.Any() )
            {
                var tracks = runningWorktracks.ToList();
                var users = tracks.Select( x => x.UserId );

                foreach ( var userName in users )
                {
                    await _hub.Value.Clients.Group( userName.ToString() ).SendAsync( "getActiveTracking", false, null, false, TextResource.API_TrackingTaskClosed )
                        .ConfigureAwait( false );
                }

                var now = DateTime.UtcNow;
                tracks.ForEach( x =>
                {
                    x.Running = false;
                    x.StoppedTime = now;
                } );

                _context.UpdateRange( tracks );
            }

            dbWorkTask.StateId = ( byte ) model.StateId;

            _context.Update( dbWorkTask );

            await _context.SaveChangesAsync( true )
                .ConfigureAwait( false );

            return new JsonResult( new OkResponse
            {
               message = "Вы успешно изменили состояние задачи" 
            } );
        }
    }
}