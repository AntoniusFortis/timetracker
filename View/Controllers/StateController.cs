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
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Timetracker.Models.Classes;
    using Timetracker.Models.Models;
    using Timetracker.Models.Responses;
    using Timetracker.View.Resources;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status403Forbidden )]
    public class StateController : ControllerBase
    {
        private readonly TimetrackerContext _context;
        private readonly JsonSerializerOptions _jsonOptions;

        public StateController(TimetrackerContext dbContext)
        {
            _context = dbContext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Получить все доступные состояния задач
        /// </summary>
        /// <returns>Список всех состояний задач</returns>
        [HttpGet]
        [ResponseCache( Duration = 360 )]
        public async Task<JsonResult> GetAll()
        {
            var states = await _context.States.AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
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
            var dbWorkTask = _context.Worktasks.FirstOrDefault(x => x.Id == model.TaskId );
            if ( dbWorkTask == null )
            {
                throw new Exception( TextResource.API_NotExistWorktaskId );
            }

            var stateExist = await _context.States.AnyAsync(x => x.Id == model.StateId )
                .ConfigureAwait( false );
            if ( !stateExist )
            {
                throw new Exception( "Неверный идентификатор состояния задачи" );
            }

            int projectId = dbWorkTask.ProjectId;
            int userId = int.Parse( User.Identity.Name );

            var linkedProject = _context.GetLinkedAcceptedProject( projectId, userId );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            dbWorkTask.StateId = ( byte ) model.StateId;

            _context.Update( dbWorkTask );

            await _context.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new OkResponse
            {
               message = "Вы успешно изменили состояние задачи" 
            } );
        }
    }
}