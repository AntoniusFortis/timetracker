/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.View.Controllers
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
    using Timetracker.Models.Extensions;
    using Timetracker.Models.Models;
    using Timetracker.Models.Responses;
    using Timetracker.View.Resources;

    [ApiController]
    [Route( "api/[controller]/[action]" )]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status400BadRequest )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status403Forbidden )]
    public class RoleController : ControllerBase
    {
        private readonly TimetrackerContext _context;
        private readonly JsonSerializerOptions _jsonOptions;

        public RoleController( TimetrackerContext context )
        {
            _context = context;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Получить все доступные роли
        /// </summary>
        /// <returns>Список всех ролей/returns>
        [HttpGet]
        [ProducesResponseType( typeof( RoleGetAllResponse ), StatusCodes.Status200OK )]
        [ResponseCache( Duration = 720 )]
        public async Task<JsonResult> GetAll()
        {
            var roles = await _context.Roles.AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult( new RoleGetAllResponse
            {
                roles = roles
            }, _jsonOptions );
        }

        /// <summary>
        /// Изменить роль пользователя в проекте
        /// </summary>
        /// <param name="model">Данные для изменения роли</param>  
        /// <returns>Проект в котором изменили роль и сам Ид роли</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>          
        [HttpPost]
        [ProducesResponseType( typeof( RoleUpdateUserResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> UpdateUser( UpdateUserModel model )
        {
            byte roleId = byte.Parse( model.rightId);
            int projectId = int.Parse( model.projectId);

            int userId = int.Parse( User.Identity.Name );

            // Проверка доступа
            var linkedProjectRequestUser = await _context.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProjectRequestUser == null || linkedProjectRequestUser.RoleId == 1 )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            // Проверка существования проекта
            var linkedProject = await _context.LinkedProjects.FirstOrDefaultAsync(x => x.User.Login == model.userLogin && x.ProjectId == projectId )
                .ConfigureAwait(false);
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NotExistLinkedProject );
            }

            linkedProject.RoleId = ( byte ) roleId;

            await _context.SaveChangesAsync( true )
                .ConfigureAwait( false );

            // Обновляем значение в кэше
            await _context.GetLinkedAcceptedProject( linkedProject.ProjectId, linkedProject.UserId, true )
                .ConfigureAwait( false );

            return new JsonResult( new RoleUpdateUserResponse
            {
                project = linkedProject.Project,
                roleId = roleId
            }, _jsonOptions );
        }

        /// <summary>
        /// Получить роль в проекте для вызвавшего пользователя
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>  
        /// <returns>Идентификатор новой роли</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>    
        [HttpGet]
        [ProducesResponseType( typeof( RoleGetRoleResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> GetRole( [FromQuery] uint? id )
        {
            int projectId = this.ParseValue( id );
            int userId = int.Parse( User.Identity.Name );

            // Проверка доступа
            var linkedProject = await _context.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait(false);
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NotExistWorktaskId );
            }

            return new JsonResult( new RoleGetRoleResponse
            {
                role = linkedProject.RoleId,
                isAdmin = linkedProject.RoleId == 1
            } );
        }
    }
}