/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.View.Middlewares
{
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware( RequestDelegate next )
        {
            _next = next;
        }

        public async Task Invoke( HttpContext httpContext )
        {
            try
            {
                await _next( httpContext );
            }
            catch ( Exception ex )
            {
                await HandleException( httpContext, ex );
            }
        }

        private static Task HandleException( HttpContext context, Exception ex )
        {
            var code = HttpStatusCode.BadRequest;

            var result = JsonConvert.SerializeObject( new {
                    status = code,
                    message = ex.Message
                } );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ( int ) code;

            return context.Response.WriteAsync( result );
        }
    }
}
