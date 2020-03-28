using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
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
    public class AddProjectView
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

        public async Task<IActionResult> GetAll()
        {
            var user = await _dbContext.GetUser(User.Identity.Name);

            var userProjects = _dbContext.AuthorizedUsers.AsNoTracking()
                .Where(x => x.UserId == user.Id)
                .Include(x => x.Project)
                .Select(x => new { x.IsSigned, x.Project });

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

            return new JsonResult(projectsView);
        }

        public async Task<IActionResult> GetProject(int? id)
        {
            if (!id.HasValue)
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
                .Where(x => x.ProjectId == id)
                .Include(x => x.User)
                .Select(x => x.User.Login)
                .ToArrayAsync();

            var response = new {
                status = HttpStatusCode.OK,
                project = project,
                users = users
            };

            return new JsonResult(response, _jsonOptions);
        }

        public async Task<IActionResult> AddProject([FromBody] AddProjectView project)
        {
            var user = await _dbContext.GetUser(User.Identity.Name);

            if (_dbContext.Projects.Any(x => x.Title == project.Project.Title))
            {
                return BadRequest();
            }

            await _dbContext.AddAsync(project.Project);
            await _dbContext.AddAsync(new AuthorizedUser
            {
                IsSigned = true,
                Project = project.Project,
                RightId = 1,
                UserId = user.Id
            });

            foreach (var userName in project.Users)
            {
                var userInviting = await _dbContext.GetUser(userName);

                if (userInviting == null)
                    continue;

                await _dbContext.AddAsync(new AuthorizedUser
                {
                    IsSigned = false,
                    Project = project.Project,
                    RightId = 1,
                    UserId = userInviting.Id
                });
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> Update([FromBody] AddProjectView updatedProject)
        {
            _dbContext.Projects.Update(updatedProject.Project);

            var user = await _dbContext.GetUser(User.Identity.Name);

            var authorizedUsers = await _dbContext.AuthorizedUsers.AsNoTracking()
                .Where(x => x.ProjectId == updatedProject.Project.Id && x.UserId != user.Id)
                .Include(x => x.User)
                .Where(x => !updatedProject.Users.Any(y => x.User.Login == y ) )
                .ToArrayAsync();

            if (authorizedUsers.Any())
                _dbContext.RemoveRange(authorizedUsers);

            foreach (var userName in updatedProject.Users)
            {
                if (userName == User.Identity.Name)
                    continue;

                var userInviting = await _dbContext.GetUser(userName);

                if (userInviting == null)
                    continue;

                await _dbContext.AddAsync(new AuthorizedUser
                {
                    IsSigned = false,
                    Project = updatedProject.Project,
                    RightId = 1,
                    UserId = userInviting.Id
                });
            }

            _dbContext.SaveChanges();

            return Ok();
        }

        public async Task<IActionResult> Remove(int? id)
        {
            if (!id.HasValue)
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.InternalServerError
                });
            }

            var project = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == id);

            if (project == null)
            {
                return new JsonResult(new
                {
                    text = "Не существует проекта с таким Id",
                    status = HttpStatusCode.BadRequest
                });
            }

            var authorizedUsers = _dbContext.AuthorizedUsers
                .Where(x => x.ProjectId == id).ToList();
            
            _dbContext.RemoveRange(authorizedUsers);

            _dbContext.Remove(project);

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                status = HttpStatusCode.OK
            });
        }
    }
}