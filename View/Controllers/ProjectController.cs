using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;

namespace View.Controllers
{
    public class CU_ProjectView
    {
        public Project Project { get; set; }
    }

    public class InviteModel
    {
        public string ProjectId { get; set; }

        public string[] Users { get; set; }
    }

    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class ProjectController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProjectController(TimetrackerContext dbcontext)
        {
            _dbContext = dbcontext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public async Task<JsonResult> GetUsers(int id)
        {
            var users = await _dbContext.AuthorizedUsers.Where(x => x.ProjectId == id)
                .Select(x => x.User.Login)
                .ToListAsync()
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK,
                users = users,
            };

            return new JsonResult(response, _jsonOptions);
        }

        [HttpPost]
        public async Task<JsonResult> Invite(InviteModel model)
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var projectId = int.Parse(model.ProjectId);

            var authorizedUsers = _dbContext.AuthorizedUsers
                .Where(x => x.ProjectId == projectId);

            var users = model.Users.Distinct();
            if (!users.Any())
            {
                return new JsonResult(new
                {
                    users = authorizedUsers.Select(x => x.User).ToList(),
                    status = HttpStatusCode.OK
                });
            }

            var dbUsers = await _dbContext.Users.Where(x => users.Any(y => y == x.Login))
                .ToListAsync()
                .ConfigureAwait(false);

            var dbLinkedUsers = await _dbContext.AuthorizedUsers
                .Where(x => x.ProjectId == projectId && x.IsSigned)
                .ToListAsync()
                .ConfigureAwait(false);

            var dbUsersIds = dbUsers.Select(x => x.Id);
            var dbLinkedIds = dbLinkedUsers.Select(x => x.Id);

            var toRemove = dbLinkedIds.Except(dbUsersIds);
            var toAdd = dbUsersIds.Except(dbLinkedIds);

            _dbContext.AuthorizedUsers.RemoveRange(dbLinkedUsers.Where(x => toRemove.Contains(x.Id)));
            await _dbContext.AuthorizedUsers.AddRangeAsync(toAdd.Select(x => new AuthorizedUser
            {
                IsSigned = false,
                ProjectId = projectId,
                RightId = 1,
                UserId = x
            })).ConfigureAwait(false);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            return new JsonResult(new
            {
                status = HttpStatusCode.OK
            });
        }

        [HttpPost]
        public async Task<JsonResult> Add([FromBody] CU_ProjectView projectView)
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            //if (await _dbContext.Projects.AsNoTracking()
            //    .AnyAsync(x => x.Title == projectView.Project.Title))
            //{
            //    return new JsonResult(new
            //    {
            //        status = HttpStatusCode.BadRequest,
            //        text = "Проект с таким именем уже существует"
            //    });
            //}

            await _dbContext.AddAsync(projectView.Project)
                .ConfigureAwait(false);

            await _dbContext.AddAsync(new AuthorizedUser
            {
                IsSigned = true,
                Project = projectView.Project,
                RightId = 1,
                UserId = user.Id
            }).ConfigureAwait(false);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            return new JsonResult(new
            {
                status = HttpStatusCode.OK
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var userProjects = _dbContext.AuthorizedUsers.AsNoTracking()
                .Where(x => x.UserId == user.Id);

            var projects = await userProjects.Where(x => x.IsSigned)
                .Select(x => x.Project)
                .OrderBy(x => x.Title)
                .ToListAsync()
                .ConfigureAwait(false);

            var notSignedProjects = await userProjects.Where(x => !x.IsSigned)
                .Select(x => x.Project)
                .OrderBy(x => x.Title)
                .ToListAsync()
                .ConfigureAwait(false);

            var projectsView = new
            {
                SignedProjects = projects,
                NotSignedProjects = notSignedProjects
            };

            return new JsonResult(projectsView, _jsonOptions);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int? id)
        {
            if (!id.HasValue)
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.InternalServerError,
                    project = (Project)null
                });
            }
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            // Проверяем, что есть доступ
            var hasAccess = await _dbContext.AuthorizedUsers.AsNoTracking()
                .AnyAsync(x => x.UserId == user.Id)
                .ConfigureAwait(false);

            if (!hasAccess)
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.InternalServerError,
                    project = (Project)null
                });
            }

            var project = await _dbContext.Projects.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            var tasks =  await _dbContext.Tasks.Where(x => x.ProjectId == id)
                .ToListAsync()
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK,
                project = project,
                tasks = tasks
            };

            return new JsonResult(response, _jsonOptions);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CU_ProjectView view)
        {
            if (view.Project == null)
            {
                return new JsonResult(new
                {
                    text = "Не существует проекта с таким Id",
                    status = HttpStatusCode.BadRequest
                });
            }

            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            if (!_dbContext.CheckAccessForProject(view.Project.Id, user))
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.Unauthorized
                }, _jsonOptions);
            }

            var project = _dbContext.Projects.FirstOrDefault(x => x.Id == view.Project.Id);
            project.Title = view.Project.Title;
            project.Description = view.Project.Description;

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                status = HttpStatusCode.OK,
                project = view.Project
            }, _jsonOptions);
        }

        /// <summary>
        /// Удалить проект
        /// </summary>
        /// <param name="Id"></param>     
        [HttpDelete]
        public async Task<IActionResult> Delete(int? Id)
        {
            var id = Id.Value;
            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            if (!_dbContext.CheckAccessForProject(id, user))
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.Unauthorized
                }, _jsonOptions);
            }

            var project = await _dbContext.Projects.SingleAsync(x => x.Id == id);

            _dbContext.Remove(project);

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                status = HttpStatusCode.OK
            }, _jsonOptions);
        }
    }
}