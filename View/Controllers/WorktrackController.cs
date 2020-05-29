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
    public class WorktrackController : ControllerBase
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
        public async Task<JsonResult> GetAll( int? id )
        {
            if ( !id.HasValue )
            {
                var response = new
                {
                    status = HttpStatusCode.OK,
                    message = "Вы не предоставили поле id"
                };

                return new JsonResult( response, _jsonOptions );
            }

            var worktracks = await _dbContext.Worktracks.Where(x => x.TaskId == id.Value && !x.Draft )
                .OrderByDescending( x => x.StartedTime )
                .Select( x => new {
                    x.Id,
                    User = x.User.Login,
                    StartedTime = x.StartedTime.ToString("G"),
                    StoppedTime = x.StoppedTime.ToString("G"),
                    TotalTime = (x.StoppedTime - x.StartedTime).ToString(@"hh\:mm\:ss")
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult( worktracks, _jsonOptions );
        }

        [HttpPost]
        public async Task<JsonResult> GetStat( WorkTrackStatModel model )
        {
            var startedDate = DateTime.Parse( model.startDate );
            var endDate = DateTime.Parse( model.endDate ).AddDays(1);

            var worktracksQuery =  _dbContext.Worktracks.Where( x => x.Task.ProjectId == model.projectId && x.StartedTime >= startedDate && x.StoppedTime <= endDate );

            if ( model.userId.HasValue && model.userId != 0 )
            {
                worktracksQuery = worktracksQuery.Where( x => x.UserId == model.userId.Value );
            }

            if ( model.taskId.HasValue && model.taskId != 0 )
            {
                worktracksQuery = worktracksQuery.Where( x => x.TaskId == model.taskId.Value );
            }

            var worktracks = await worktracksQuery.Select( x => new
            {
                x.Id,
                User = x.User.Login,
                Task = x.Task.Title,
                StartedTime = x.StartedTime.ToString("G"),
                StoppedTime = x.StoppedTime.ToString("G"),
                TotalTime = (x.StoppedTime - x.StartedTime).ToString(@"hh\:mm\:ss")
            })
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult( worktracks, _jsonOptions );
        }
    }
}
