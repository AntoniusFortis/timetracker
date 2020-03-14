using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

    [ApiController]
    public class ProjectController : Controller
    {
        private readonly TimetrackerContext _dbContext;

        public ProjectController(TimetrackerContext authorizedUsersRepository)
        {
            _dbContext = authorizedUsersRepository;
        }

        [HttpGet("[controller]")]
        public async Task<IActionResult> Get()
        {
            var user = await _dbContext.GetUser(HttpContext.User.Identity.Name);

            var projectsView = new ProjectsView();

            if (user == null)
            {
                return new JsonResult(projectsView) { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            var userProjects = _dbContext.AuthorizedUsers.AsNoTracking().Where(x => x.UserId == user.Id);

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

            projectsView = new ProjectsView
            {
                SignedProjects = projects,
                NotSignedProjects = notSignedProjects
            };

            return new JsonResult(projectsView);
        }

        [HttpPost("[controller]/AddProject")]
        public async Task<IActionResult> AddProject([FromBody] AddProjectView project)
        {
            var user = await _dbContext.GetUser(HttpContext.User.Identity.Name);

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
    }
}