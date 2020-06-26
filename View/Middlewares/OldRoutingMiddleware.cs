/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.View.Middlewares
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;

    public class OldRoutingMiddleware
    {
        private readonly RequestDelegate _next;

        public OldRoutingMiddleware( RequestDelegate next )
        {
            _next = next;
        }

        public async Task Invoke( HttpContext httpContext )
        {
            var path = httpContext.Request.Path.Value;

            if ( path.Contains( "api/project/GetMyRight", StringComparison.OrdinalIgnoreCase ) )
            {
                httpContext.Request.Path =  new PathString( path.Replace( "api/project/GetMyRight", "api/role/getrole" ) );
            }

            if ( path.Contains( "api/project/UpdateUser", StringComparison.OrdinalIgnoreCase ) )
            {
                httpContext.Request.Path = new PathString( path.Replace( "api/project/UpdateUser", "api/role/updateUser" ) );
            }

            if ( path.Contains( "api/task/UpdateState", StringComparison.OrdinalIgnoreCase ) )
            {
                httpContext.Request.Path = new PathString( path.Replace( "api/task/UpdateState", "api/state/update" ) );
            }

            await _next( httpContext );
        }
    }
}
