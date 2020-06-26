using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;
using Timetracker.Models.Classes;
using Timetracker.Models.Extensions;
using Timetracker.View.Hubs;
using Timetracker.View.Middlewares;

namespace View
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddDatabase(Configuration);
            services.AddControllers();
            services.AddAuth();
            services.AddSignalR();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            services.AddSwagger( xmlPath );

            services.AddSpaStaticFiles( configuration =>
             {
                 configuration.RootPath = "ClientApp/build";
             } );
        }

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env, TimetrackerContext dbContext )
        {
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI( c =>
             {
                 c.SwaggerEndpoint( "/swagger/v1/swagger.json", "Time Tracker API" );
             } );

            if ( !env.IsDevelopment() )
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            //if ( !env.IsDevelopment() )
            {
                app.UseSpaStaticFiles();
            }

            app.UseMiddleware( typeof( OldRoutingMiddleware ) );
            app.UseMiddleware( typeof( ExceptionHandlerMiddleware ) );
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints( endpoints =>
             {
                 endpoints.MapControllers();
                 endpoints.MapHub<TrackingHub>( "/trackingHub", x =>
                 {
                     x.ApplicationMaxBufferSize = 64;
                     x.TransportMaxBufferSize = 64;
                     x.LongPolling.PollTimeout = TimeSpan.FromMinutes( 1 );
                     x.Transports = HttpTransportType.LongPolling | HttpTransportType.WebSockets;
                 } );
             } );

            app.UseSpa( spa =>
                 {
                     spa.Options.SourcePath = "ClientApp";

                     if ( env.IsDevelopment() )
                     {
                         spa.UseReactDevelopmentServer( npmScript: "start" );
                     }
                 } );
        }
    }
}
