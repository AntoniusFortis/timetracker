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
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;

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

        public MyPageController(TimetrackerContext dbContext, IWebHostEnvironment appEnvironment)
        {
            _dbContext = dbContext;
            _appEnvironment = appEnvironment;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public async Task<JsonResult> Get()
        {
            var user = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK,
                user
            };

            return new JsonResult(response, _jsonOptions);
        }

        [HttpPost]
        public async Task<JsonResult> Update(User model)
        {
            var dbUser = await _dbContext.GetUserAsync(User.Identity.Name)
                .ConfigureAwait(false);

            //var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Id == dbUser.Id)
            //    .ConfigureAwait(false);
            _dbContext.Update(model);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK,
                oldUser = dbUser,
                newUser = model
            };

            return new JsonResult(response, _jsonOptions);
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<JsonResult> UpdateAvatar([FromForm] IFormFile avatar)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Login == User.Identity.Name)
                .ConfigureAwait(false);

            user.AvatarPath = await GetAvatar(avatar, user.Login)
                .ConfigureAwait(false);

            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            var response = new
            {
                status = HttpStatusCode.OK
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