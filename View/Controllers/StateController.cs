using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Classes;

namespace Timetracker.View.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StateController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public StateController(TimetrackerContext dbContext)
        {
            _dbContext = dbContext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public async Task<JsonResult> GetAll()
        {
            var statesArray = await _dbContext.States.AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult(new
            {
                status = HttpStatusCode.OK,
                states = statesArray,
            }, _jsonOptions);
        }
    }
}