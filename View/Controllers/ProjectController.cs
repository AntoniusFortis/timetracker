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
            var userName = HttpContext.User.Identity.Name;

            var user = await _dbContext.GetUser(userName);

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
        public async Task<IActionResult> AddProject([FromBody] Project project)
        {
            var userName = HttpContext.User.Identity.Name;

            var user = await _dbContext.GetUser(userName);

            await _dbContext.AddAsync(project);
            await _dbContext.AddAsync(new AuthorizedUser
            {
                IsSigned = true,
                Project = project,
                RightId = 1,
                UserId = user.Id
            });

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}