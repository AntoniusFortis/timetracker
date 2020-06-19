using System.Net;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Responses
{
    public class RoleUpdateUserResponse
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;

        public Project project { get; set; }

        public byte roleId { get; set; }
    }
}
