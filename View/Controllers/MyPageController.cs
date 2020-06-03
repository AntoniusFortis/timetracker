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

namespace Timetracker.View.Controllers
{
    [Produces("application/json", new[] { "multipart/form-data" })]
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
            var user = await _dbContext.GetUserAsync(User.Identity.Name, true )
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK,
                user
            };

            return new JsonResult(response, _jsonOptions);
        }

        [HttpPost]
        public async Task<JsonResult> Update( MyPageModel model )
        {
            var login = User.Identity.Name;
            var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Login == login)
                .ConfigureAwait(false);

            var passwordChanged = !string.IsNullOrEmpty(model.Pass);
            if ( passwordChanged )
            {
                var password = PasswordHelpers.EncryptPassword(model.CurrentPass, user.Salt, 1024);
                if ( !PasswordHelpers.SlowEquals( password, user.Pass ) )
                {
                    var responseUnauthorized = new
                    {
                        status = HttpStatusCode.Unauthorized
                    };

                    return new JsonResult( responseUnauthorized, _jsonOptions );
                }

                var salt = PasswordHelpers.GenerateSalt(16);
                var hash = PasswordHelpers.EncryptPassword(model.Pass, salt, 1024);
                user.Pass = hash;
                user.Salt = salt;
            }

            user.FirstName = model.FirstName;
            user.Surname = model.Surname;
            user.MiddleName = model.MiddleName;
            user.City = model.City;
            user.Email = model.Email;
            user.BirthDate = model.BirthDate;

            _dbContext.Update(user);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            var resultUser = new
            {
                user.Login,
                user.FirstName,
                user.Surname,
                user.MiddleName,
                user.City,
                user.Email,
                user.BirthDate
            };
            await _dbContext.GetUserAsync( User.Identity.Name, true );

            var response = new
            {
                status = HttpStatusCode.OK,
                newUser = resultUser,
                passwordChanged = passwordChanged
            };

            return new JsonResult(response, _jsonOptions);
        }

        //[HttpDelete]
        //public async Task<JsonResult> Delete()
        //{
        //    var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Login == User.Identity.Name)
        //        .ConfigureAwait(false);

        //    _dbContext.Remove(user);

        //    await _dbContext.SaveChangesAsync()
        //        .ConfigureAwait(false);

        //    var response = new
        //    {
        //        status = HttpStatusCode.OK
        //    };

        //    return new JsonResult(response, _jsonOptions);
        //}
    }
}