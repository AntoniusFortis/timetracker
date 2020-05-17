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
    public class WorktrackController : Controller
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public WorktrackController(TimetrackerContext dbContext)
        {
            _dbContext = dbContext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public async Task<JsonResult> GetAll(int? worktaskId)
        {
            var worktracks = await _dbContext.Worktracks
                .Where(x => x.TaskId == worktaskId.Value && !x.Draft )
                .OrderByDescending( x => x.StartedTime )
                .Select( x => new {
                    Id = x.Id,
                    User = x.User.Login,
                    StartedTime = x.StartedTime.ToString("G"),
                    StoppedTime = x.StoppedTime.ToString("G"),
                    TotalTime = (x.StoppedTime - x.StartedTime).ToString(@"hh\:mm\:ss")
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult(worktracks, _jsonOptions);
        }
    }
}
