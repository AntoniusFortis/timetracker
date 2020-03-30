using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;

namespace Timetracker.View.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class TaskController : Controller
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public TaskController(TimetrackerContext authorizedUsersRepository)
        {
            _dbContext = authorizedUsersRepository;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IActionResult> Get(int? Id)
        {
            var id = Id.Value;

            var worktask = await _dbContext.Tasks.SingleAsync(x => x.Id == id);

            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            // Проверяем, что есть доступ
            var hasAccess = await _dbContext.AuthorizedUsers.AsNoTracking()
                .AnyAsync(x => x.UserId == user.Id && x.ProjectId == worktask.ProjectId);

            if (!hasAccess)
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.InternalServerError,
                    project = (Project)null,
                    worktask = (WorkTask)null
                });
            }

            return new JsonResult(new
            {
                status = HttpStatusCode.InternalServerError,
                project = worktask.Project,
                worktask = worktask
            }, _jsonOptions);
        }
    }
}