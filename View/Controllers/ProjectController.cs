using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;

namespace View.Controllers
{
    public class ProjectsView
    {
        public Project[] SignedProjects { get; set; }

        public Project[] NotSignedProjects { get; set; }
    }

    public class AddProjectView
    {
        public Project Project { get; set; }

        public string[] Users { get; set; }
    }

    public class InviteUsersView
    {
        public int ProjectId { get; set; }

        public string[] Users { get; set; }
    }

    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class ProjectController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;

        public ProjectController(TimetrackerContext authorizedUsersRepository)
        {
            _dbContext = authorizedUsersRepository;
        }

        public async Task<IActionResult> GetProjects()
        {
            var user = await _dbContext.GetUser(User.Identity.Name);

            var userProjects = _dbContext.AuthorizedUsers.AsNoTracking()
                .Where(x => x.UserId == user.Id);

            var projects = await userProjects
                .Where(x => x.IsSigned)
                .Include(x => x.Project)
                .Select(x => x.Project)
                .ToArrayAsync();

            var notSignedProjects = await userProjects
                .Where(x => !x.IsSigned)
                .Include(x => x.Project)
                .Select(x => x.Project)
                .ToArrayAsync();

            var projectsView = new ProjectsView
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

            var project = await _dbContext.Projects
                .FirstOrDefaultAsync(x => x.Id == id);

            var users = await _dbContext.AuthorizedUsers
                .Where(x => x.ProjectId == id)
                .Include(x => x.User)
                .Select(x => new { x.User.Login })
                .ToArrayAsync();

            var response = new {
                status = HttpStatusCode.OK,
                project = project,
                users = users
            };

            return new JsonResult(response);
        }

        public async Task<IActionResult> AddProject([FromBody] AddProjectView project)
        {
            var user = await _dbContext.GetUser(User.Identity.Name);

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

        public async Task<IActionResult> InviteUsers([FromBody] InviteUsersView view)
        {
            var project = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == view.ProjectId);

            foreach (var userName in view.Users)
            {
                var userInviting = await _dbContext.GetUser(userName);

                await _dbContext.AddAsync(new AuthorizedUser
                {
                    IsSigned = false,
                    Project = project,
                    RightId = 1,
                    UserId = userInviting.Id
                });
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}