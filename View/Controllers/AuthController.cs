using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;
using Timetracker.View;

namespace View.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;

        public AuthController(TimetrackerContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("[controller]/Auth")]
        public async Task<IActionResult> Auth([FromBody] User user)
        {
            var dbUser = await _dbContext.GetUser(user.Login);

            if (dbUser == null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            var salt = dbUser.Salt;
            var hash = PasswordHelpers.EncryptPassword(user.Pass, salt, 1024);

            if (!PasswordHelpers.SlowEquals(hash, dbUser.Pass))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            await Authenticate(user.Login);

            return Ok("Вы успешно авторизовались!");
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

        [HttpGet("[controller]/IsAuth")]
        public IActionResult IsAuth()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Ok();
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPost("[controller]/signup")]
        public async Task<IActionResult> SignUp([FromBody] User user)
        {
            var userExists = (await _dbContext.GetUser(user.Login)) != null;

            if (userExists)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            var salt = PasswordHelpers.GenerateSalt(16);
            var hash = PasswordHelpers.EncryptPassword(user.Pass, salt, 1024);

            user.Pass = hash;
            user.Salt = salt;

            await _dbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return Ok("Вы успешно зарегистрировались!");
        }

        [HttpGet("[controller]/SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}
