using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;
using Timetracker.Entities.Responses;
using Timetracker.View;

namespace View.Controllers
{
    [Produces("application/json", new[] { "multipart/form-data" })]
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public AccountController(TimetrackerContext dbContext)
        {
            _dbContext = dbContext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpPost]
        public async Task<IActionResult> SignIn( SignInModel view )
        {
            var dbUser = await _dbContext.GetUserAsync(view.Login).ConfigureAwait(false);
            if ( dbUser == null )
            {
                return Unauthorized();
            }

            var password = PasswordHelpers.EncryptPassword(view.Pass, dbUser.Salt, 1024);
            if ( !PasswordHelpers.SlowEquals( password, dbUser.Pass ) )
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, dbUser.Login)
                };

            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            var now = DateTime.Now;
            var jwt = new JwtSecurityToken(
                    issuer: TimetrackerAuthorizationOptions.ISSUER,
                    audience: TimetrackerAuthorizationOptions.AUDIENCE,
                    notBefore: now,
                    claims: claimsIdentity.Claims,
                    expires: now.Add(TimeSpan.FromDays(TimetrackerAuthorizationOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(TimetrackerAuthorizationOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var user = new
            {
                dbUser.Id,
                dbUser.Login,
                dbUser.FirstName,
                dbUser.MiddleName,
                dbUser.Surname,
                dbUser.Email,
                dbUser.BirthDate,
                dbUser.City
            };

            var response = new
            {
                status = HttpStatusCode.OK,
                access_token = encodedJwt,
                user =  user
            };

            return new JsonResult( response );
        }

        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetCurrentUser()
        {
            var currentUser = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK,
                user = currentUser
            };

            return new JsonResult( response, _jsonOptions );
        }

        [HttpPost]
        public async Task<IActionResult> SignUp( AccountResponse view )
        {
            var userExist = _dbContext.UserExists(view.Login);

            if ( userExist )
            {
                return BadRequest( new
                {
                    text = "Пользователь с таким именем уже существует."
                } );
            }

            var salt = PasswordHelpers.GenerateSalt(16);
            var hash = PasswordHelpers.EncryptPassword(view.Pass, salt, 1024);

            var user = new User
            {
                Login = view.Login,
                Pass = hash,
                Salt = salt,
                FirstName = view.FirstName,
                Surname = view.Surname,
                MiddleName = view.MiddleName,
                City = view.City,
                BirthDate = view.BirthDate,
                Email = view.Email
            };

            await _dbContext.AddAsync( user ).ConfigureAwait( false );
            await _dbContext.SaveChangesAsync().ConfigureAwait( false );

            var response = new
            {
                status = HttpStatusCode.OK,
                text = "Вы успешно зарегистрировались!"
            };

            return new JsonResult( response );
        }
    }
}
