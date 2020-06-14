using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Extensions;
using Timetracker.Entities.Hubs;

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
            //services.AddCors( x => {
            //    x.AddDefaultPolicy( policy =>
            //    {
            //        policy.AllowAnyOrigin()
            //        .AllowAnyHeader()
            //        .AllowAnyMethod();
            //    } );
            //} );
            services.AddMemoryCache();
            services.AddDatabase(Configuration);
            services.AddControllers();
            services.AddAuth();
            services.AddSignalR();
            services.AddSwagger();
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env, TimetrackerContext dbContext )
        {
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI( c =>
             {
                 c.SwaggerEndpoint( "/swagger/v1/swagger.json", "Time Tracker API" );
             } );

            app.UseExceptionHandler( "/Error" );

            if ( !env.IsDevelopment() )
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSpaStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseCors();


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
