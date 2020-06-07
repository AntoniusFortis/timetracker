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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Entity;
using Timetracker.Entities.Models;
using Timetracker.Entities.Responses;
using Timetracker.Entities;
using Timetracker.Models.Models;
using Timetracker.Models.Helpers;
using Timetracker.Models.Response;
using Timetracker.Models.Responses;

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

                    var dbUser = await _dbContext.GetUserAsync(login, true)
                        .ConfigureAwait(false);

                    var dbToken = await _dbContext.Tokens.FirstOrDefaultAsync( x => x.Id == dbUser.TokenId);

                    if ( dbToken.RefreshToken == model.RefreshToken )
                    {
                        await TokenHelpers.GenerateToken( login, dbToken, _dbContext );

                        return new JsonResult( new TokenResponse
                        {
                            status = HttpStatusCode.OK,
                            access_token = dbToken.AccessToken,
                            refresh_token = dbToken.RefreshToken,
                            expired_in = ( dbToken.TokenExpiredDate - now ).TotalSeconds
                        } );
                    }
                    else
                    {
                        return new JsonResult( new ErrorResponse
                        {
                            message = "Неверный Refresh Token"
                        } );
                    }
                }
                else
                {
                    return new JsonResult( new ErrorResponse
                    {
                        message = "Срок действия вашего токена ещё не истёк"
                    } );
                }
            }

            return new JsonResult( new ErrorResponse
            {
                message = "Срок действия вашего токена ещё не истёк"
            } );
        }
        [HttpPost]
        [ResponseCache( Duration = 100 )]
        public async Task<IActionResult> SignIn( [FromBody] SignInModel model )
        {
            if ( string.IsNullOrEmpty( model.login.Trim() ) || string.IsNullOrEmpty( model.pass.Trim() ) )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Отсутствует логин или пароль"
                } );
            }

            var dbUser = await _dbContext.GetUserAsync(model.login, true)
                .ConfigureAwait(false);
            if ( dbUser == null )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Пользователя с таким логином не существует"
                } );
            }

            var password = PasswordHelpers.EncryptPassword(model.pass, dbUser.Salt);
            if ( !PasswordHelpers.SlowEquals( password, dbUser.Pass ) )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Неправильный логин или пароль"
                } );
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

            var dbToken = await _dbContext.Tokens.FirstOrDefaultAsync( x => x.Id == dbUser.TokenId)
                .ConfigureAwait(false);

            if ( dbToken != null && dbToken.TokenExpiredDate >= now )
            {
                return new JsonResult( new SignInResponse
                {
                    status = HttpStatusCode.OK,
                    access_token = dbToken.AccessToken,
                    refresh_token = dbToken.RefreshToken,
                    expired_in = ( dbToken.TokenExpiredDate - now ).TotalSeconds,
                    user = user
                } );
            }

            bool isFirst = false;
            // Если пользователь авторизуется впервые
            if ( dbToken == null )
            {
                dbToken = new Token();
                isFirst = true;
            }

            await TokenHelpers.GenerateToken( dbUser.Login, dbToken, _dbContext, isFirst )
                .ConfigureAwait( false );

            dbUser.TokenId = dbToken.Id;

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new SignInResponse
            {
                status = HttpStatusCode.OK,
                access_token = dbToken.AccessToken,
                refresh_token = dbToken.RefreshToken,
                expired_in = ( dbToken.TokenExpiredDate - now ).TotalSeconds,
                user = user
            } );
        }

        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetCurrentUser()
        {
            var currentUser = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                user = currentUser
            }, _jsonOptions );
        }

        [HttpPost]
        public async Task<JsonResult> SignUp( [FromBody] SignUpModel model )
        {
            var userExists = await _dbContext.UserExists(model.Login)
                .ConfigureAwait(false);

            if ( userExists )
            {
                return new JsonResult( new ErrorResponse
                {
                    message = "Пользователь с таким логином уже существует"
                } );
            }

            var salt = PasswordHelpers.GenerateSalt();
            var hash = PasswordHelpers.EncryptPassword(model.Pass, salt);

            var user = new User
            {
                Login = model.Login,
                Pass = hash,
                Salt = salt,
                FirstName = model.FirstName,
                Surname = model.Surname,
                MiddleName = model.MiddleName,
                City = model.City,
                BirthDate = model.BirthDate,
                Email = model.Email
            };

            await _dbContext.AddAsync( user ).ConfigureAwait( false );
            await _dbContext.SaveChangesAsync().ConfigureAwait( false );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                message = "Регистрация пройдена успешно"
            } );
        }
    }
}
