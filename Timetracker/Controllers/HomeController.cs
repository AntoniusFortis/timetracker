using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;
using Timetracker.Models;
using System.Linq;

namespace Timetracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly AuthorizedUsersRepository _authorizedUsersRepository;
        private readonly ProjectRepository _projectRepository;

        public HomeController(UserRepository userRepository, AuthorizedUsersRepository authorizedUsersRepository, ProjectRepository projectRepository)
        {
            _userRepository = userRepository;
            _authorizedUsersRepository = authorizedUsersRepository;
            _projectRepository = projectRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registration(User user)
        {
            var users = _userRepository.GetAll();

            var userExists = await users.AnyAsync(x => x.Name == user.Name);

            if (userExists)
            {
                ModelState.AddModelError("Name", "Пользователь с таким именем уже существует!");
                return View();
            }

            _userRepository.Add(user);
            _userRepository.SaveAll();

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ViewResult Auth()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Auth(User user)
        {
            var users = _userRepository.GetAll();

            if (!await users.AnyAsync(x => x.Name == user.Name))
            {
                ModelState.AddModelError("Name", "Такого пользователя не существует!");
                return View();
            }

            await Authenticate(user.Name);

            return RedirectToAction("Projects");
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };

            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
