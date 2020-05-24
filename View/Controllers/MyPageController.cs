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
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IMemoryCache _cache;

        public MyPageController(TimetrackerContext dbContext, IWebHostEnvironment appEnvironment, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _appEnvironment = appEnvironment;
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
        public async Task<JsonResult> Update(AccountResponse model)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Login == model.Login)
                .ConfigureAwait(false);

            if ( !string.IsNullOrEmpty(model.Pass) )
            {
                var salt = PasswordHelpers.GenerateSalt(16);
                var hash = PasswordHelpers.EncryptPassword(model.Pass, salt, 1024);
                user.Pass = hash;
                user.Salt = salt;
            }

            user.Login = model.Login;
            user.FirstName = model.FirstName;
            user.Surname = model.Surname;
            user.MiddleName = model.MiddleName;
            user.City = model.City;
            user.Email = model.Email;

            var isDateParsed = DateTime.TryParse(model.BirthDate, out var birthDate);

            if (isDateParsed)
            {
                user.BirthDate = birthDate;
            }

            _dbContext.Update(user);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK,
                newUser = model
            };

            return new JsonResult(response, _jsonOptions);
        }

        [HttpGet]
        public async Task<FileStreamResult> GetAvatar()
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            string path = "/Resources/" + user.Id.ToString() + user.AvatarPath;

            var image = System.IO.File.OpenRead(path);
            return File( image, "image/jpeg" );

            //return File( path, "application/octet-stream", "hello.txt" );
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<JsonResult> UpdateAvatar([FromForm] IFormFile avatar)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Login == User.Identity.Name)
                .ConfigureAwait(false);

            if ( !string.IsNullOrEmpty( user.AvatarPath ) && System.IO.File.Exists( _appEnvironment.WebRootPath + user.AvatarPath ) )
            {
                System.IO.File.Delete( _appEnvironment.WebRootPath + user.AvatarPath );
            }

            user.AvatarPath = await GetAvatar(avatar, user.Id.ToString())
                .ConfigureAwait(false);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK,
                path = user.AvatarPath
            };

            return new JsonResult(response, _jsonOptions);
        }

        [HttpDelete]
        public async Task<JsonResult> Delete()
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Login == User.Identity.Name)
                .ConfigureAwait(false);

            _dbContext.Remove(user);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK
            };

            return new JsonResult(response, _jsonOptions);
        }

        private async Task<string> GetAvatar(IFormFile avatar, string login)
        {
            if (avatar == null)
            {
                return null;
            }

            string path = "/Resources/" + login + avatar.FileName;
            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
            {
                await avatar.CopyToAsync(fileStream)
                    .ConfigureAwait(false);
            }

            return path;
        }
    }
}