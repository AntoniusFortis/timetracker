using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Timetracker.Models.Classes;
using Timetracker.Models.Helpers;
using Timetracker.Models.Models;
using Timetracker.Models.Responses;
using Timetracker.View.Resources;

namespace Timetracker.Entities.Controllers
{
    [Produces( "application/json" )]
    [ApiController]
    [Route( "api/[controller]/[action]" )]
    public class MyPageController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public MyPageController( TimetrackerContext dbContext )
        {
            _dbContext = dbContext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public async Task<JsonResult> Get()
        {
            var dbUser = await _dbContext.GetUserAsync( User.Identity.Name )
                .ConfigureAwait(false);

            var IV = dbUser.IV;

            var firstName = PasswordHelpers.DecryptData( dbUser.FirstName, IV );
            var surName = PasswordHelpers.DecryptData( dbUser.Surname, IV );
            var middleName = string.IsNullOrEmpty( dbUser.MiddleName ) ? string.Empty : PasswordHelpers.DecryptData( dbUser.MiddleName, IV );
            var birthDate = string.IsNullOrEmpty( dbUser.BirthDate ) ? string.Empty : PasswordHelpers.DecryptData( dbUser.BirthDate, IV );
            var city = string.IsNullOrEmpty( dbUser.City ) ? string.Empty : PasswordHelpers.DecryptData( dbUser.City, IV );
            var email = PasswordHelpers.DecryptData( dbUser.Email, IV );

            var user = new
            {
                dbUser.Id,
                dbUser.Login,
                FirstName = firstName,
                Surname = surName,
                MiddleName = middleName,
                BirthDate = birthDate,
                City = city,
                Email = email
            };

            return new JsonResult( new MypageGetResponse
            {
                user = user
            }, _jsonOptions );
        }

        /// <summary>
        /// Изменить персональную информацию о пользователе
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> Update( MyPageModel model )
        {
            var dbUser = await _dbContext.GetUserAsync( User.Identity.Name, true )
                .ConfigureAwait(false);

            var passwordChanged = !string.IsNullOrEmpty(model.Pass);
            if ( passwordChanged )
            {
                var password = PasswordHelpers.EncryptPassword(model.CurrentPass, dbUser.Pass.Salt);
                if ( !PasswordHelpers.SlowEquals( password, dbUser.Pass.Password ) )
                {
                    throw new Exception( TextResource.API_NoAccess );
                }

                var salt = PasswordHelpers.GenerateSalt();
                var hash = PasswordHelpers.EncryptPassword(model.Pass, salt);
                dbUser.Pass.Password = hash;
                dbUser.Pass.Salt = salt;
            }

            var IV = dbUser.IV;
            var firstName = PasswordHelpers.EncryptData( model.FirstName, IV );
            var surName = PasswordHelpers.EncryptData( model.Surname, IV );
            var email = PasswordHelpers.EncryptData( model.Email, IV );
            var middleName = string.IsNullOrEmpty( model.MiddleName ) ? null : PasswordHelpers.EncryptData( model.MiddleName, IV );
            var birthdate = string.IsNullOrEmpty( model.BirthDate ) ? null : PasswordHelpers.EncryptData( model.BirthDate, IV );
            var city = string.IsNullOrEmpty( model.City ) ? null : PasswordHelpers.EncryptData( model.City, IV );

            dbUser.FirstName = firstName;
            dbUser.Surname = surName;
            dbUser.MiddleName = middleName;
            dbUser.City = city;
            dbUser.Email = email;
            dbUser.BirthDate = birthdate;

            _dbContext.Update( dbUser );

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait( false );

            var user = new
            {
                dbUser.Login,
                FirstName = model.FirstName,
                Surname = model.Surname,
                MiddleName = model.MiddleName,
                City = model.City,
                Email = model.Email,
                BirthDate = model.BirthDate
            };

            await _dbContext.GetUserAsync( User.Identity.Name, true )
                .ConfigureAwait( false );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                newUser = user,
                passwordChanged = passwordChanged
            }, _jsonOptions );
        }
    }
}