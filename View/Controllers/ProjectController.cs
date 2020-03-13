using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    [Route("[controller]/[action]")]
    public class ProjectController : Controller
    {
        private readonly TimetrackerContext _dbContext;
        private readonly IMemoryCache _cache;

        public ProjectController(TimetrackerContext authorizedUsersRepository, IMemoryCache cache)
        {
            _dbContext = authorizedUsersRepository;
            _cache = cache;
        }

        public async Task<IActionResult> Projects()
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
            
            return new OkObjectResult(Json(projectsView));
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

            return RedirectToAction("Projects");
        }

    }
}