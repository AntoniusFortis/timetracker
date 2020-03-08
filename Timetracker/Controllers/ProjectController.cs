using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;
using Timetracker.Models;

namespace Timetracker.Controllers
{
    public class ProjectController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly AuthorizedUsersRepository _authorizedUsersRepository;
        private readonly ProjectRepository _projectRepository;

        public ProjectController(UserRepository userRepository, AuthorizedUsersRepository authorizedUsersRepository, ProjectRepository projectRepository)
        {
            _userRepository = userRepository;
            _authorizedUsersRepository = authorizedUsersRepository;
            _projectRepository = projectRepository;
        }

        public async Task<IActionResult> Projects()
        {
            var userName = HttpContext.User.Identity.Name;
            var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Name == userName);

            if (user == null)
            {
                return View();
            }

            var userProjects = _authorizedUsersRepository.GetAll().Where(x => x.UserId == user.Id);

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

            return View(projectsView);
        }

        [HttpGet]
        public IActionResult AddProject()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProject(Project project)
        {
            var userName = HttpContext.User.Identity.Name;
            var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Name == userName);

            _projectRepository.Add(project);
            _authorizedUsersRepository.Add(new AuthorizedUser { IsSigned = true, Project = project, RightId = 1, UserId = user.Id });

            _projectRepository.SaveAll();

            return RedirectToAction("Projects");
        }

    }
}