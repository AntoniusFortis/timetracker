using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

            if (user == null)
            {
                return View();
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

            var projectsView = new ProjectsView
            {
                SignedProjects = projects,
                NotSignedProjects = notSignedProjects
            };

            var json = Json(projectsView);

            return new OkObjectResult(json);
        }

        [HttpPost("[controller]/AddProject")]
        public async Task<IActionResult> AddProject([FromForm] Project project, [FromBody] string[] users)
        {
            var userName = HttpContext.User.Identity.Name;
            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Name == userName);

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