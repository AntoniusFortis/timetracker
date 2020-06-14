using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Hubs;
using Timetracker.Entities.Models;
using Timetracker.Models.Models;
using Timetracker.Models.Response;
using Timetracker.Models.Responses;

namespace View.Controllers
{
    public class ProjectUserModel
    {
        public string ProjectId { get; set; }

        public int UserId { get; set; }
    }

    public class ProjectIdUserNameModel
    {
        public string ProjectId { get; set; }

        public string UserName { get; set; }
    }

    [ApiController]
    [Authorize]
    [Route( "api/[controller]/[action]" )]
    public class ProjectController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IMemoryCache _cache;
        private readonly IHubContext<TrackingHub> _hub;

        public ProjectController( TimetrackerContext dbcontext, IMemoryCache cache, IHubContext<TrackingHub> hub )
        {
            _dbContext = dbcontext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _hub = hub;
        }

        [HttpGet]
        public async Task<JsonResult> GetUsers( [FromQuery] uint? id )
        {
            if ( !id.HasValue )
            {
                return new JsonResult( new
                {
                    status = HttpStatusCode.BadRequest,
                    message = "Вы должны ввести идентификатор проекта"
                } );
            }

            if ( !int.TryParse( id.Value.ToString(), out var projectId ) )
            {
                return new JsonResult( new
                {
                    status = HttpStatusCode.BadRequest,
                    message = "Недопустимый формат идентификатора"
                } );
            }

            var users = await _dbContext.LinkedProjects.Where(x => x.ProjectId == projectId)
                .Select(x => new {
                    Id = x.UserId,
                    login = x.User.Login,
                    right = x.Right
                } )
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                users = users,
            }, _jsonOptions );
        }

        [HttpPost]
        public async Task<JsonResult> UpdateUser( UpdateUserModel model )
        {
            byte rId = byte.Parse( model.rightId);
            int pId = int.Parse( model.projectId);

            var authorizedUsers = await _dbContext.LinkedProjects.SingleOrDefaultAsync(x => x.User.Login == model.userLogin && x.ProjectId == pId )
                .ConfigureAwait(false);

            authorizedUsers.RightId = (byte)rId;

            await _dbContext.SaveChangesAsync().ConfigureAwait( false );

            var response = new
            {
                status = HttpStatusCode.OK
            };

            return new JsonResult( response, _jsonOptions );
        }

        [HttpGet]
        public async Task<JsonResult> GetMyRight( int id )
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name, true )
                .ConfigureAwait(false);

            var authorizedUsers = await _dbContext.LinkedProjects.SingleAsync(x => x.UserId == user.Id && x.ProjectId == id)
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK,
                isAdmin = authorizedUsers.RightId == 1
            };

            return new JsonResult( response, _jsonOptions );
        }

        [HttpPost]
        public async Task<JsonResult> Reject( uint? id )
        {
            if ( !int.TryParse( id.Value.ToString(), out var projectId ) )
            {
                return new JsonResult( new
                {
                    status = HttpStatusCode.BadRequest,
                    message = "Недопустимый формат идентификатора"
                } );
            }

            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var authorizedUsers = await _dbContext.LinkedProjects.FirstOrDefaultAsync(x => x.UserId == user.Id && x.ProjectId == projectId)
                .ConfigureAwait(false);

            if ( authorizedUsers == null )
            {
                return new JsonResult( new
                {
                    status = HttpStatusCode.BadRequest,
                    message = "Вы не были приглашены в этот проект, либо проект не существует"
                } );
            }

            if ( authorizedUsers.Accepted )
            {
                return new JsonResult( new
                {
                    status = HttpStatusCode.BadRequest,
                    message = "Вы уже приняли приглашение в проект"
                } );
            }

            _dbContext.Remove( authorizedUsers );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                message = "Вы отказались от приглашения в проект"
            } );
        }

        [HttpPost]
        public async Task<JsonResult> Accept( [FromBody] InviteAcceptModel model )
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var projectId = model.projectId;

            var authorizedUsers = await _dbContext.LinkedProjects.SingleAsync(x => x.UserId == user.Id && x.ProjectId == projectId)
                .ConfigureAwait(false);

            authorizedUsers.Accepted = true;

            _dbContext.Update( authorizedUsers );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                messge = "Вы успешно приняли приглашение в проект"
            } );
        }

        [HttpPost]
        public async Task<JsonResult> RemoveUserFromProject( ProjectUserModel model )
        {
            var projectId = int.Parse( model.ProjectId );
            var userId = model.UserId;

            var au = await _dbContext.LinkedProjects
                .FirstOrDefaultAsync( x => x.ProjectId == projectId && x.UserId == userId )
                .ConfigureAwait(false);

            _dbContext.LinkedProjects.Remove( au );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK
            } );
        }

        [HttpPost]
        public async Task<JsonResult> AddUserToProject( [FromBody] ProjectIdUserNameModel model )
        {
            var projectId = int.Parse( model.ProjectId );

            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var hasAccess = await _dbContext.LinkedProjects.AnyAsync( x => x.ProjectId == projectId && x.User.Id == user.Id  && x.RightId != 1 )
                .ConfigureAwait(false);

            if ( !hasAccess )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "У вас нет прав для добавления нового участника в проект"
                } );
            }

            var newUserId = await _dbContext.Users.AsNoTracking()
                .Where( x => x.Login == model.UserName )
                .Select( x => x.Id )
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if ( _dbContext.LinkedProjects.AsNoTracking().Any( x => x.UserId == newUserId && x.ProjectId == projectId ) )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Этот пользователь уже есть в проекте"
                } );
            }

            await _dbContext.LinkedProjects.AddAsync( new LinkedProject
            {
                ProjectId = projectId,
                UserId = newUserId,
                RightId = 1,
                Accepted = false
            } )
                .ConfigureAwait( false );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK
            } );
        }

        /// <summary>
        /// Добавить проект
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> Add( [FromBody] AddProjectModel model )
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            if ( await _dbContext.Projects.AsNoTracking()
                .AnyAsync( x => x.Title == model.title ) )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Проект с таким именем уже существует"
                } );
            }

            var addedProject = await _dbContext.AddAsync(new Project
            {
                Title = model.title,
                Description = model.description
            })
                .ConfigureAwait(false);

            await _dbContext.SaveChangesAsync()
                   .ConfigureAwait( false );

            await _dbContext.AddAsync( new LinkedProject
            {
                Accepted = true,
                ProjectId = addedProject.Entity.Id,
                RightId = 3,
                UserId = user.Id
            } )
                .ConfigureAwait( false );


            await _dbContext.SaveChangesAsync()
                           .ConfigureAwait( false );

            return new JsonResult( new ProjectAddResponse
            {
                project = new
                {
                    addedProject.Entity.Id,
                    addedProject.Entity.Title,
                    addedProject.Entity.Description
                }
            } );
        }

        /// <summary>
        /// Получить все проекты
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAll()
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var userProjects = _dbContext.LinkedProjects.AsNoTracking()
                .Where(x => x.UserId == user.Id);

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
                status = HttpStatusCode.OK,
                acceptedProjects = acceptedProjects ?? null,
                notAcceptedProjects = notAcceptedProjects ?? null
            }, _jsonOptions );
        }

        /// <summary>
        /// Получить информацию о проекте
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> Get( [FromQuery] uint? id )
        {
            if ( !id.HasValue )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Вы должны ввести идентификатор проекта"
                } );
            }

            if ( !int.TryParse( id.Value.ToString(), out var projectId ) )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Недопустимый формат идентификатора"
                } );
            }

            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            // Проверяем, что есть доступ
            var linkedProject = _dbContext.GetLinkedProjectForUser( projectId, user.Id );
            if ( linkedProject == null || !linkedProject.Accepted )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "У вас нет доступа к данному проекту"
                } );
            }

            var project = await _dbContext.Projects.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            var tasks =  await _dbContext.Worktasks.AsNoTracking()
                .Where(x => x.ProjectId == id)
                .OrderBy( x => x.StateId )
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Description,
                    x.State
                })
                .ToListAsync()
                .ConfigureAwait(false);

            var users = await _dbContext.LinkedProjects.AsNoTracking()
                .Where(x => x.ProjectId == id )
                .Select(x => new
                {
                    Id = x.UserId,
                    login = x.User.Login,
                    right = x.Right
                } )
                .ToListAsync()
                .ConfigureAwait(false);

            var caller = new
            {
                id = user.Id,
                login = user.Login,
                right = linkedProject.Right
            };

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                project = project,
                tasks = tasks,
                caller = caller,
                users = users
            }, _jsonOptions );
        }

        /// <summary>
        /// Обновить проект
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> Update( [FromBody] ProjectUpdateModel model )
        {
            if ( model.Project == null )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Не существует проекта с таким Id"
                } );
            }

            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var hasAccess = await _dbContext.LinkedProjects.AnyAsync( x => x.ProjectId == model.Project.Id && x.User.Id == user.Id  && x.RightId != 1 )
                .ConfigureAwait(false);

            if ( !hasAccess )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "У вас нет прав для изменения проекта"
                } );
            }

            var project = await _dbContext.Projects.FirstOrDefaultAsync( x => x.Id == model.Project.Id )
                .ConfigureAwait(false);

            project.Title = model.Project.Title;
            project.Description = model.Project.Description;

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                project = model.Project
            }, _jsonOptions );
        }

        /// <summary>
        /// Удалить проект
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>     
        [HttpDelete]
        public async Task<JsonResult> Delete( [FromQuery] uint? id )
        {
            if ( !id.HasValue )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Вы должны ввести идентификатор проекта"
                } );
            }

            if ( !int.TryParse( id.Value.ToString(), out var projectId ) )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Недопустимый формат идентификатора"
                } );
            }

            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var hasAccess = await _dbContext.LinkedProjects.AnyAsync( x => x.ProjectId == projectId && x.User.Id == user.Id  && x.RightId != 1 )
                .ConfigureAwait(false);

            if ( !hasAccess )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "У вас нет прав для удаления проекта"
                } );
            }

            var linkedProjects = _dbContext.LinkedProjects.Where( x => x.ProjectId == projectId );
            _dbContext.LinkedProjects.RemoveRange( linkedProjects );

            var project = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == projectId)
                .ConfigureAwait(false);
            _dbContext.Projects.Remove( project );

            // Если на задачу кто-то тречит - заставить их прекратить
            var runningWorktracksUsers = _dbContext.Worktracks.Where( x => x.Worktask.ProjectId == projectId && x.Running)
                .Select( x => x.User.Login )
                .ToHashSet();

            foreach ( var userName in runningWorktracksUsers )
            {
                await _hub.Clients.Group( userName ).SendAsync( "getActiveTracking", false, null, false, "Задача была удалена и больше не отслеживается" )
                    .ConfigureAwait( false );
            }

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new 
            {
                message = "Вы успешно удалили проект",
                project = project
            }, _jsonOptions );
        }
    }
}