using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        //public async Task<IActionResult> Logout()
        //{
        //    await HttpContext.SignOutAsync( JwtBearerDefaults.AuthenticationScheme );
        //    return new JsonResult(null);
        //}

        [HttpPost]
        public async Task<JsonResult> Token( [FromBody] TokenModel model )
        {
            if ( !string.IsNullOrEmpty( model.AccessToken ) )
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken( model.AccessToken );
                var now = DateTime.UtcNow;

                if ( now >= token.ValidTo )
                {
                    var login = token.Claims.FirstOrDefault().Value;
                    var dbUser = await _dbContext.GetUserAsync(login, true);

                    if ( dbUser.RefreshToken == model.RefreshToken )
                    {
                        GenerateToken( login, dbUser );

                        var responseToken = new
                        {
                            status = HttpStatusCode.OK,
                            access_token = dbUser.AccessToken,
                            refresh_token = dbUser.RefreshToken,
                            expired_in = (dbUser.TokenExpiredDate - now).Value.TotalSeconds
                        };

                        return new JsonResult( responseToken );
                    }
                }
            }

            var response = new
            {
                status = HttpStatusCode.InternalServerError
            };

            return new JsonResult( response );
        }

        private void GenerateToken(string login, User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, login)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            var tokenLifetime = TimeSpan.FromMinutes( 1 );
            var now = DateTime.UtcNow;
            var expiredIn = now.Add( tokenLifetime );
            var jwt = new JwtSecurityToken(
                    issuer: TimetrackerAuthorizationOptions.ISSUER,
                    audience: TimetrackerAuthorizationOptions.AUDIENCE,
                    notBefore: now,
                    claims: claimsIdentity.Claims,
                    expires: expiredIn,
                    signingCredentials: new SigningCredentials(TimetrackerAuthorizationOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var access_token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");

            user.AccessToken = access_token;
            user.RefreshToken = refresh_token;
            user.TokenExpiredDate = expiredIn;

            _dbContext.SaveChanges();
        }
        [HttpPost]
        public async Task<IActionResult> SignIn( [FromBody] SignInModel view )
        {
            var dbUser = await _dbContext.GetUserAsync(view.Login, true)
                .ConfigureAwait(false);

            if ( dbUser == null )
            {
                return Unauthorized();
            }

            var password = PasswordHelpers.EncryptPassword(view.Pass, dbUser.Salt, 1024);
            if ( !PasswordHelpers.SlowEquals( password, dbUser.Pass ) )
            {
                return Unauthorized();
            }

            var now = DateTime.UtcNow;

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

            if ( dbUser.TokenExpiredDate >= now )
            {
                var responseUser = new
                {
                    status = HttpStatusCode.OK,
                    access_token = dbUser.AccessToken,
                    refresh_token = dbUser.RefreshToken,
                    expired_in = (dbUser.TokenExpiredDate - now).Value.TotalSeconds,
                    user = user
                };

                return new JsonResult( responseUser );
            }

            GenerateToken( dbUser.Login, dbUser );

            var responseWithToken = new
            {
                status = HttpStatusCode.OK,
                access_token = dbUser.AccessToken,
                refresh_token = dbUser.RefreshToken,
                expired_in =  (dbUser.TokenExpiredDate - now).Value.TotalSeconds,
                user = user
            };

            return new JsonResult( responseWithToken );
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
        public async Task<IActionResult> SignUp( AccountModel view )
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
