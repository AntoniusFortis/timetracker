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
    public class TaskView
    {
        public WorkTask worktask { get; set; }
    }

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

        [HttpPost]
        public async Task<IActionResult> Add(TaskView view)
        {
            var worktask = view.worktask;
            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            if (!_dbContext.CheckAccessForProject(worktask.ProjectId, user))
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.Unauthorized
                }, _jsonOptions);
            }

            await _dbContext.AddAsync(worktask);

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                status = HttpStatusCode.OK,
                worktask = worktask
            }, _jsonOptions);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int? Id)
        {
            var id = Id.Value;

            var worktask = await _dbContext.Tasks.SingleAsync(x => x.Id == id);

            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            // Проверяем, что есть доступ
            if (!_dbContext.CheckAccessForProject(worktask.ProjectId, user))
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
                status = HttpStatusCode.OK,
                project = worktask.Project,
                worktask = worktask
            }, _jsonOptions);
        }

        [HttpPost]
        public async Task<IActionResult> Update(TaskView view)
        {
            var worktask = view.worktask;
            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            if (!_dbContext.CheckAccessForProject(worktask.Project.Id, user))
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.Unauthorized
                }, _jsonOptions);
            }

            var dbWorkTask = _dbContext.Tasks.FirstOrDefault(x => x.Id == view.worktask.Id);
            dbWorkTask.Title = view.worktask.Title;
            dbWorkTask.Description = view.worktask.Description;
            dbWorkTask.Duration = view.worktask.Duration;
            dbWorkTask.StateId = view.worktask.State.Id;
            dbWorkTask.State = view.worktask.State;

            _dbContext.Update(dbWorkTask);
            
            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                status = HttpStatusCode.OK,
                worktask = worktask
            }, _jsonOptions);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? Id)
        {
            var id = Id.Value;
            var user = await _dbContext.GetUserAsync(User.Identity.Name);

            var dbWorkTask = await _dbContext.Tasks.SingleAsync(x => x.Id == id);

            if (!_dbContext.CheckAccessForProject(dbWorkTask.ProjectId, user))
            {
                return new JsonResult(new
                {
                    status = HttpStatusCode.Unauthorized
                }, _jsonOptions);
            }

            _dbContext.Remove(dbWorkTask);

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                status = HttpStatusCode.OK
            }, _jsonOptions);
        }
    }
}