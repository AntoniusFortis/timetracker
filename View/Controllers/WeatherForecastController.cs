using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;

namespace View.Controllers
{

    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly TimetrackerContext _dbContext;

        public WeatherForecastController(TimetrackerContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("[controller]")]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
 

        [HttpPost]
        [Route("[controller]/Auth")]
        public async Task<IActionResult> Auth([FromForm] User user)
        {
            var users = _dbContext.Users.AsNoTracking();

            if (!await users.AnyAsync(x => x.Name == user.Name))
            {
                ModelState.AddModelError("Name", "Такого пользователя не существует!");
                return StatusCode(500);
            }

            await Authenticate(user.Name);

            return Ok();
        }
        [Route("[controller]/IsAuth")]
        public IActionResult IsAuth()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);

            }
        }

        [Route("[controller]/Registration")]
        [HttpPost]
        public async Task<IActionResult> Registration(User user)
        {
            var users = _dbContext.Users.AsNoTracking();

            var userExists = await users.AnyAsync(x => x.Name == user.Name);

            if (userExists)
            {
                ModelState.AddModelError("Name", "Пользователь с таким именем уже существует!");
                return StatusCode(500);
            }

            await _dbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("[controller]/SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
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
    }
}
