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
    [Route( "api/[controller]/[action]" )]
    public class RoleController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public RoleController( TimetrackerContext dbContext )
        {
            _dbContext = dbContext;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        [ResponseCache( Duration = 360 )]
        public async Task<JsonResult> GetAll()
        {
            var roles = await _dbContext.Roles.AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return new JsonResult( new
            {
                status = HttpStatusCode.OK,
                roles = roles,
            }, _jsonOptions );
        }
    }
}