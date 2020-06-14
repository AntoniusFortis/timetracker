using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;
using Timetracker.Entities.Responses;
using Timetracker.Models.Responses;

namespace Timetracker.Entities.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MyPageController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IMemoryCache _cache;

        public MyPageController( TimetrackerContext dbContext, IMemoryCache cache )
        {
            _dbContext = dbContext;
            _cache = cache;

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

            var user = new
            {
                dbUser.Id,
                dbUser.Login,
                dbUser.FirstName,
                dbUser.Surname,
                dbUser.MiddleName,
                dbUser.BirthDate,
                dbUser.City,
                dbUser.Email
            };

            return new JsonResult( new MypageGetResponse
            {
                user = user
            }, _jsonOptions );
        }

        [HttpPost]
        public async Task<JsonResult> Update( MyPageModel model )
        {
            var login = User.Identity.Name;
            var dbUser = await _dbContext.Users.SingleOrDefaultAsync(x => x.Login == login)
                .ConfigureAwait(false);

            var passwordChanged = !string.IsNullOrEmpty(model.Pass);
            if ( passwordChanged )
            {
                var password = PasswordHelpers.EncryptPassword(model.CurrentPass, dbUser.Salt);
                if ( !PasswordHelpers.SlowEquals( password, dbUser.Pass ) )
                {
                    var responseUnauthorized = new
                    {
                        status = HttpStatusCode.Unauthorized
                    };

                    return new JsonResult( responseUnauthorized, _jsonOptions );
                }

                var salt = PasswordHelpers.GenerateSalt();
                var hash = PasswordHelpers.EncryptPassword(model.Pass, salt);
                dbUser.Pass = hash;
                dbUser.Salt = salt;
            }

            dbUser.FirstName = model.FirstName;
            dbUser.Surname = model.Surname;
            dbUser.MiddleName = model.MiddleName;
            dbUser.City = model.City;
            dbUser.Email = model.Email;
            dbUser.BirthDate = model.BirthDate;

            _dbContext.Update(dbUser);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            var user = new
            {
                dbUser.Login,
                dbUser.FirstName,
                dbUser.Surname,
                dbUser.MiddleName,
                dbUser.City,
                dbUser.Email,
                dbUser.BirthDate
            };

            await _dbContext.GetUserAsync( User.Identity.Name, true );

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                newUser = user,
                passwordChanged = passwordChanged
            }, _jsonOptions);
        }
    }
}