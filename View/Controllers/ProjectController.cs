using System.Linq;
using System.Net;
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

        public string[] Users { get; set; }
    }

    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class ProjectController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProjectController(TimetrackerContext authorizedUsersRepository)
        {
            _dbContext = authorizedUsersRepository;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            var userProjects = _dbContext.AuthorizedUsers.AsNoTracking()
                .Select(x => new { x.IsSigned, x.Project, x.UserId })
                .Where(x => x.UserId == user.Id);

            var projects = await userProjects
                .Where(x => x.IsSigned)
                .Select(x => x.Project)
                .ToArrayAsync();

            var notSignedProjects = await userProjects
                .Where(x => !x.IsSigned)
                .Select(x => x.Project)
                .ToArrayAsync();

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
            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            // Проверяем, что есть доступ
            var hasAccess = await _dbContext.AuthorizedUsers.AsNoTracking()
                .AnyAsync(x => x.UserId == user.Id);

            if (!hasAccess)
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.InternalServerError,
                    project = (Project)null
                });
            }

            var project = await _dbContext.Projects.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            var users = await _dbContext.AuthorizedUsers
                .AsNoTracking()
                .Select(x => new
                {
                    Login = x.User.Login,
                    ProjectId = x.ProjectId
                })
                .Where(x => x.ProjectId == id && x.Login != user.Login)
                .Select(x => x.Login)
                .ToArrayAsync();

            var response = new
            {
                status = HttpStatusCode.OK,
                project = project,
                users = users
            };

            return new JsonResult(response, _jsonOptions);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CU_ProjectView projectView)
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            if (await _dbContext.Projects.AsNoTracking()
                .AnyAsync(x => x.Title == projectView.Project.Title))
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.BadRequest,
                    text = "Проект с таким именем уже существует"
                });
            }

            await _dbContext.AddAsync(projectView.Project);

            await _dbContext.AddAsync(new AuthorizedUser
            {
                IsSigned = true,
                Project = projectView.Project,
                RightId = 1,
                UserId = user.Id
            });

            foreach (var userName in projectView.Users)
            {
                var userInviting = await _dbContext.GetUserAsync(userName);

                if (userInviting == null)
                    continue;

                await _dbContext.AddAsync(new AuthorizedUser
                {
                    IsSigned = false,
                    Project = projectView.Project,
                    RightId = 1,
                    UserId = userInviting.Id
                });
            }

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                status = HttpStatusCode.OK
            });
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

            var updatedProject = _dbContext.Update(view.Project);
            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            var authorizedUsers = _dbContext.AuthorizedUsers.AsNoTracking()
                .Where(x => x.ProjectId == view.Project.Id && x.User.Id != user.Id);

            var toremove = authorizedUsers
                .Where(x => view.Users.Any(y => x.User.Login != y));

            if (toremove.Any())
                _dbContext.RemoveRange(toremove);

            foreach (var userName in view.Users)
            {
                if (userName == User.Identity.Name)
                    continue;

                var userInviting = await _dbContext.GetUserAsync(userName);

                if (userInviting == null)
                    continue;

                await _dbContext.AddAsync(new AuthorizedUser
                {
                    IsSigned = false,
                    Project = view.Project,
                    RightId = 1,
                    UserId = userInviting.Id
                });
            }

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                project = updatedProject,
                users = authorizedUsers.ToArray(),
                status = HttpStatusCode.OK
            });
        }

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