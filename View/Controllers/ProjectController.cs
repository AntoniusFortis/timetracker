/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace View.Controllers
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
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.Caching.Memory;
    using Timetracker.Models.Classes;
    using Timetracker.Models.Entities;
    using Timetracker.Models.Extensions;
    using Timetracker.Models.Models;
    using Timetracker.Models.Responses;
    using Timetracker.View.Hubs;
    using Timetracker.View.Resources;

    [ApiController]
    [Authorize]
    [Route( "api/[controller]/[action]" )]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status403Forbidden )]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status400BadRequest )]
    public class ProjectController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Lazy<IHubContext<TrackingHub>> _hub;

        public ProjectController( TimetrackerContext dbcontext, IHubContext<TrackingHub> hub )
        {
            _dbContext = dbcontext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _hub = new Lazy<IHubContext<TrackingHub>>( hub );
        }

        /// <summary>
        /// Получить всех пользователей проекта
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>  
        /// <returns>Список всех пользователей проекта</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>   
        [HttpGet]
        [ProducesResponseType( typeof( ProjectGetUsersResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> GetUsers( [FromQuery] uint? id )
        {
            int projectId = this.ParseValue( id );
            int userId = int.Parse( User.Identity.Name );

            // Проверяем, что есть доступ
            var linkedProject = _dbContext.GetLinkedAcceptedProject( projectId, userId );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            var users = await _dbContext.LinkedProjects.Where(x => x.ProjectId == projectId)
                .Select(x => new {
                    Id = x.UserId,
                    login = x.User.Login,
                    right = x.Role
                } )
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult( new ProjectGetUsersResponse
            {
                users = users
            }, _jsonOptions );
        }

        /// <summary>
        /// Отменить приглашение в проект
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>  
        /// <returns>Сообщение об отмене приглашения</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>  
        [HttpPost]
        [ProducesResponseType( typeof( OkResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Reject( [FromQuery] uint? id )
        {
            int projectId = this.ParseValue( id );
            int userId = int.Parse( User.Identity.Name );

            // Проверяем, что есть доступ
            var linkedProject = _dbContext.GetLinkedProjectForUser( projectId, userId );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_YouAreNotInvited );
            }

            if ( linkedProject.Accepted )
            {
                throw new Exception( TextResource.API_InviteAccepted );
            }

            _dbContext.Remove( linkedProject );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new OkResponse
            {
                message = "Вы отказались от приглашения в проект"
            } );
        }

        /// <summary>
        /// Принять приглашение в проект
        /// </summary>
        /// <param name="model">Данные для осуществления приглашения</param>  
        /// <returns>Данные о проекте в который пригласили</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата данных</response>  
        [HttpPost]
        [ProducesResponseType( typeof( ProjectAcceptResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Accept( [FromBody] InviteAcceptModel model )
        {
            int projectId = model.projectId;
            int userId = int.Parse( User.Identity.Name );

            // Проверяем, что есть доступ
            var linkedProject = _dbContext.GetLinkedProjectForUser( projectId, userId );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_YouAreNotInvited );
            }

            if ( linkedProject.Accepted )
            {
                throw new Exception( TextResource.API_InviteAccepted );
            }

            linkedProject.Accepted = true;

            _dbContext.Update( linkedProject );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new ProjectAcceptResponse
            {
                project = linkedProject.Project
            } );
        }

        /// <summary>
        /// Удалить пользователя из проекта
        /// </summary>
        /// <param name="model">Данные для осуществления удаления пользователя из проекта</param>  
        /// <returns>Данные о проекте из которого удалили</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата данных</response>  
        [HttpPost]
        public async Task<JsonResult> RemoveUserFromProject( ProjectUserModel model )
        {
            var projectId = int.Parse( model.ProjectId );
            int userId = int.Parse( User.Identity.Name );

            // Проверяем, что есть доступ
            var linkedProject = await _dbContext.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null || linkedProject.RoleId == 1 )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            // Если на задачу тречит удаляемый пользователь - останавливаем
            var runningWorktracksUsers = _dbContext.Worktracks.FirstOrDefault( x => x.Worktask.ProjectId == projectId && x.UserId == model.UserId && x.Running );
            if ( runningWorktracksUsers != null )
            {
                await _hub.Value.Clients.Group( User.Identity.Name ).SendAsync( "getActiveTracking", false, null, false, "У вас больше нет доступа к данной задаче" )
                    .ConfigureAwait( false );
                runningWorktracksUsers.Running = false;
                runningWorktracksUsers.StoppedTime = DateTime.UtcNow;

                _dbContext.Update( runningWorktracksUsers );
            }

            var linkedProjectToDelete = _dbContext.GetLinkedProjectForUser( projectId, model.UserId );
            if ( linkedProjectToDelete == null )
            {
                throw new Exception( "Данного пользователя не существует в проекте" );
            }

            _dbContext.LinkedProjects.Remove( linkedProjectToDelete );

            await _dbContext.SaveChangesAsync( true )
                .ConfigureAwait( false );

            await _dbContext.GetLinkedAcceptedProject( projectId, model.UserId, true )
                .ConfigureAwait( false );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                project = linkedProject.Project
            } );
        }

        /// <summary>
        /// Добавить пользователя из проекта
        /// </summary>
        /// <param name="model">Данные для осуществления добавления пользователя в проект</param> 
        /// <returns>Данные о проекте</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата данных</response> 
        [HttpPost]
        public async Task<JsonResult> AddUserToProject( [FromBody] ProjectIdUserNameModel model )
        {
            var projectId = int.Parse( model.ProjectId );
            int userId = int.Parse( User.Identity.Name );

            // Проверяем, что есть доступ
            var linkedProject = await _dbContext.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null || linkedProject.RoleId == 1 )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            var newUser = await _dbContext.Users.AsNoTracking()
                .Where( x => x.Login == model.UserName )
                .Select( x => new { x.Id, x.Login } )
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            // Проверяем что пользователь вообще существует
            if ( newUser == null )
            {
                throw new Exception( "Пользователь с таким логином не существует" );
            }

            var linkedProjectNewUser = _dbContext.GetLinkedProjectForUser( projectId, newUser.Id );
            if ( linkedProjectNewUser != null )
            {
                throw new Exception( "Этот пользователь уже есть в проекте" );
            }

            await _dbContext.LinkedProjects.AddAsync( new LinkedProject
            {
                ProjectId = projectId,
                UserId = newUser.Id,
                RoleId = 1,
                Accepted = false
            } )
                .ConfigureAwait( false );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                project = linkedProject.Project
            } );
        }

        /// <summary>
        /// Получить все проекты
        /// </summary>
        /// <returns>Информация обо всех проектах, доступных пользователю</returns>
        [HttpGet]
        [ProducesResponseType( typeof( ProjectGetAllResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> GetAll()
        {
            int userId = int.Parse( User.Identity.Name );

            var userProjects = _dbContext.LinkedProjects.AsNoTracking()
                .Where(x => x.UserId == userId );

            var acceptedProjects = await userProjects.Where( x => x.Accepted )
                    .Select( x => x.Project )
                    .OrderBy( x => x.Title )
                    .ToListAsync()
                    .ConfigureAwait( false );

            var notAcceptedProjects = await userProjects.Where( x => !x.Accepted )
                .Select( x => x.Project )
                .OrderBy( x => x.Title )
                .ToListAsync()
                .ConfigureAwait( false );

            return new JsonResult( new ProjectGetAllResponse
            {
                acceptedProjects = acceptedProjects,
                notAcceptedProjects = notAcceptedProjects
            }, _jsonOptions );
        }

        /// <summary>
        /// Получить информацию о проекте
        /// </summary>
        /// <param name="id">Идентификатор проекта</param> 
        /// <returns>Информация о проекте и его задачах</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>   
        [HttpGet]
        [ProducesResponseType( typeof( ProjectGetResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Get( [FromQuery] uint? id )
        {
            int projectId = this.ParseValue( id );

            var project = await _dbContext.Projects.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if ( project == null )
            {
                throw new Exception( TextResource.API_NotExistProjectId );
            }

            int userId = int.Parse( User.Identity.Name );

            // Проверяем, что есть доступ
            var linkedProject = await _dbContext.GetLinkedAcceptedProject( projectId, userId );
            if ( linkedProject == null )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            var userLogin = await _dbContext.Users.Where( x => x.Id == userId )
                .Select( x => x.Login )
                .FirstOrDefaultAsync();

            var tasks =  await _dbContext.Worktasks
                .Where(x => x.ProjectId == id)
                .OrderBy( x => x.StateId )
                .OrderByDescending( x => x.CreatedDate )
                .ToListAsync()
                .ConfigureAwait(false);

            var users = await _dbContext.LinkedProjects.AsNoTracking()
                .Where(x => x.ProjectId == id )
                .Select( x => new
                {
                    Id = x.UserId,
                    login = x.User.Login,
                    right = x.Role
                } )
                .ToListAsync()
                .ConfigureAwait(false);

            var caller = new
            {
                Id = userId,
                login = userLogin,
                right = linkedProject.Role
            };

            return new JsonResult( new ProjectGetResponse
            {
                project = project,
                tasks = tasks,
                caller = caller,
                users = users
            }, _jsonOptions );
        }

        /// <summary>
        /// Добавить проект
        /// </summary>
        /// <param name="model">Данные о создаваемой проекте</param> 
        /// <returns>Информация о созданном проекте</returns>
        [HttpPost]
        [ProducesResponseType( typeof( ProjectUpdateResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Add( [FromBody] AddProjectModel model )
        {
            if ( model.title.Length < 5 || model.title.Length > 50 )
            {
                throw new Exception( "Неверная длина названия проекта" );
            }

            if ( model.description.Length > 250 )
            {
                throw new Exception( "Неверная длина описания проекта" );
            }

            int userId = int.Parse( User.Identity.Name );

            var addedProject = await _dbContext.AddAsync(new Project
            {
                Title = model.title,
                Description = model.description
            })
                .ConfigureAwait(false);

            await _dbContext.SaveChangesAsync( true )
                   .ConfigureAwait( false );

            await _dbContext.AddAsync( new LinkedProject
            {
                Accepted = true,
                ProjectId = addedProject.Entity.Id,
                RoleId = 3,
                UserId = userId
            } )
                .ConfigureAwait( false );


            await _dbContext.SaveChangesAsync( true )
                .ConfigureAwait( false );

            return new JsonResult( new ProjectUpdateResponse { project = addedProject.Entity } );
        }

        /// <summary>
        /// Обновить проект
        /// </summary>
        /// <param name="model">Данные о создаваемой проекте</param> 
        /// <returns>Информация об изменённом проекте</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>  
        [HttpPost]
        [ProducesResponseType( typeof( ProjectUpdateResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Update( [FromBody] ProjectUpdateModel model )
        {
            int projectId = model.Project.Id;
            int userId = int.Parse( User.Identity.Name );

            // Проверяем доступ
            var linkedProject = await _dbContext.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null || linkedProject.RoleId == 1 )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            var project = await _dbContext.Projects.FirstOrDefaultAsync( x => x.Id == projectId )
                .ConfigureAwait(false);

            if ( project == null )
            {
                throw new Exception( TextResource.API_NotExistProjectId );
            }

            project.Title = model.Project.Title;
            project.Description = model.Project.Description;

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new ProjectUpdateResponse { project = project } );
        }

        /// <summary>
        /// Удалить проект
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>  
        /// <returns>Информация об удалённом проекте</returns>
        /// <response code="403">Отсутствие доступа</response>
        /// <response code="400">Ошибка формата идентификатора</response>  
        [HttpDelete]
        [ProducesResponseType( typeof( ProjectUpdateResponse ), StatusCodes.Status200OK )]
        public async Task<JsonResult> Delete( [FromQuery] uint? id )
        {
            int projectId = this.ParseValue( id );
            int userId = int.Parse( User.Identity.Name );

            // Проверяем доступ
            var linkedProject = await _dbContext.GetLinkedAcceptedProject( projectId, userId )
                .ConfigureAwait( false );
            if ( linkedProject == null || linkedProject.RoleId == 1 )
            {
                throw new Exception( TextResource.API_NoAccess );
            }

            // Получае проект по указанному ИД
            var project = await _dbContext.Projects.FirstOrDefaultAsync( x => x.Id == projectId )
                .ConfigureAwait(false);
            if ( project == null )
            {
                throw new Exception( TextResource.API_NotExistProjectId );
            }

            // Если на задачу кто-то тречит - заставить их прекратить
            var runningWorktracksUsers = _dbContext.Worktracks.Where( x => x.Worktask.ProjectId == projectId && x.Running )
                .ToList();
            if ( runningWorktracksUsers.Any() )
            {
                foreach ( var userName in runningWorktracksUsers )
                {
                    await _hub.Value.Clients.Group( userName.UserId.ToString() ).SendAsync( "getActiveTracking", false, null, false, TextResource.SignalR_TaskIsRemoved )
                        .ConfigureAwait( false );
                }

                var now = DateTime.UtcNow;
                runningWorktracksUsers.ForEach( x =>
                {
                    x.Running = false;
                    x.StoppedTime = now;
                } );

                _dbContext.UpdateRange( runningWorktracksUsers );
            }

            var linkedProjects = _dbContext.LinkedProjects.Where( x => x.ProjectId == projectId );

            // Сбрасываем кэш
            var cache = (IMemoryCache)Request.HttpContext.RequestServices.GetService( typeof(IMemoryCache) );
            foreach ( var lp in linkedProjects.ToList() )
            {
                cache.Remove( $"LinkedAcceptedProject:{projectId}:{lp.UserId}" );
            }

            _dbContext.LinkedProjects.RemoveRange( linkedProjects );

            _dbContext.Projects.Remove( project );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( project );
        }
    }
}