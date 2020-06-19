/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

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
using Timetracker.Entities.Models;
using Timetracker.Entities.Responses;
using Timetracker.Entities;
using Timetracker.Models.Models;
using Timetracker.Models.Helpers;
using Timetracker.Models.Responses;
using Timetracker.Models.Classes;
using Timetracker.Models.Entities;

namespace View.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]/[action]")]
    [ProducesResponseType( typeof( ErrorResponse ), StatusCodes.Status400BadRequest )]
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

        /// <summary>
        /// Получение нового токена
        /// </summary>
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
                    var userId = token.Claims.FirstOrDefault().Value;
                    var intUserId = int.Parse( userId );

                    var dbUser = await _dbContext.Users.FirstOrDefaultAsync( x => x.Id == intUserId );
                    var dbToken = await _dbContext.Tokens.FirstOrDefaultAsync( x => x.Id == dbUser.TokenId);

                    if ( dbToken.RefreshToken == model.RefreshToken )
                    {
                        await TokenHelpers.GenerateToken( userId, dbToken, _dbContext );

                        return new JsonResult( new TokenResponse
                        {
                            access_token = dbToken.AccessToken,
                            refresh_token = dbToken.RefreshToken,
                            expired_in = ( dbToken.TokenExpiredDate - now ).TotalSeconds
                        } );
                    }
                    else
                    {
                        throw new Exception( "Неверный Refresh Token" );
                    }
                }
                else
                {
                    throw new Exception( "Срок действия вашего токена ещё не истёк" );
                }
            }

            throw new Exception( "Срок действия вашего токена ещё не истёк" );
        }

        /// <summary>
        /// Авторизация пользователей
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SignIn( [FromBody] SignInModel model )
        {
            if ( string.IsNullOrEmpty( model.login.Trim() ) || string.IsNullOrEmpty( model.pass.Trim() ) )
            {
                throw new Exception( "Отсутствует логин или пароль" );
            }

            var dbUser = await _dbContext.Users.FirstOrDefaultAsync( x => x.Login == model.login )
                .ConfigureAwait(false);
            if ( dbUser == null )
            {
                throw new Exception( "Пользователя с таким логином не существует" );
            }

            var password = PasswordHelpers.EncryptPassword(model.pass, dbUser.Pass.Salt);
            if ( !PasswordHelpers.SlowEquals( password, dbUser.Pass.Password ) )
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

            await TokenHelpers.GenerateToken( dbUser.Id.ToString(), dbToken, _dbContext, isFirst )
                .ConfigureAwait( false );

            dbUser.TokenId = dbToken.Id;

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            return new JsonResult( new SignInResponse
            {
                access_token = dbToken.AccessToken,
                refresh_token = dbToken.RefreshToken,
                expired_in = ( dbToken.TokenExpiredDate - now ).TotalSeconds,
                user = user
            } );
        }

        /// <summary>
        /// Получить объект авторизованного пользователя
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetCurrentUser()
        {
            int userId = int.Parse( User.Identity.Name );
            var user = await _dbContext.Users.FirstOrDefaultAsync( x => x.Id == userId )
                .ConfigureAwait( false );

            return new JsonResult( new GetCurrentUserResponse
            {
                user = user
            }, _jsonOptions );
        }

        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> SignUp( [FromBody] SignUpModel model )
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync( x => x.Login == model.Login )
                .ConfigureAwait(false);
            if ( dbUser != null )
            {
                throw new Exception( "Пользователь с таким логином уже существует" );
            }

            var salt = PasswordHelpers.GenerateSalt();
            var hash = PasswordHelpers.EncryptPassword(model.Pass, salt);

            var IV = PasswordHelpers.GetIV();
            var firstName = PasswordHelpers.EncryptData( model.FirstName, IV );
            var surName = PasswordHelpers.EncryptData( model.Surname, IV );
            var email = PasswordHelpers.EncryptData( model.Email, IV );
            var middleName = string.IsNullOrEmpty( model.MiddleName ) ? null : PasswordHelpers.EncryptData( model.MiddleName, IV );
            var birthdate = string.IsNullOrEmpty( model.BirthDate ) ? null : PasswordHelpers.EncryptData( model.BirthDate, IV );
            var city = string.IsNullOrEmpty( model.City ) ? null : PasswordHelpers.EncryptData( model.City, IV );

            var user = new User
            {
                Login = model.Login,
                Pass = new Pass
                {
                    Password = hash,
                    Salt = salt
                },
                IV = IV,
                FirstName = firstName,
                Surname = surName,
                MiddleName = middleName,
                City = city,
                BirthDate = birthdate,
                Email = email
            };

            await _dbContext.AddAsync( user )
                .ConfigureAwait( false );
            await _dbContext.SaveChangesAsync( true )
                .ConfigureAwait( false );

            return new JsonResult( new OkResponse
            {
                message = "Регистрация пройдена успешно"
            } );
        }
    }
}
