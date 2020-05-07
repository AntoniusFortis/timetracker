using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Timetracker.Entities.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using Timetracker.View.Hubs;

namespace View
{
    public static class IServiceCollectionExtented
    {
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TimetrackerContext>(x =>
                x.UseSqlServer(configuration.GetConnectionString("Timetracker")), contextLifetime: ServiceLifetime.Scoped);
        }

        public static void AddAuth(this IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(options =>
                   {
                       options.RequireHttpsMetadata = false;
                       options.TokenValidationParameters = new TokenValidationParameters
                       {
                           ValidateIssuer = true,
                           ValidIssuer = TimetrackerAuthorizationOptions.ISSUER,

                           ValidateAudience = true,
                           ValidAudience = TimetrackerAuthorizationOptions.AUDIENCE,

                           ValidateLifetime = true,

                           IssuerSigningKey = TimetrackerAuthorizationOptions.GetSymmetricSecurityKey(),
                           ValidateIssuerSigningKey = true,
                       };
                       options.Events = new JwtBearerEvents
                       {
                           OnMessageReceived = context =>
                           {
                               var accessToken = context.Request.Query["access_token"];

                               var path = context.HttpContext.Request.Path;
                               if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/trackingHub")))
                               {
                                   context.Token = accessToken;
                               }
                               return System.Threading.Tasks.Task.CompletedTask;
                           }
                       };
                   });
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddDatabase(Configuration);
            services.AddControllers();
            services.AddAuth();
            services.AddSignalR();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Timetracker API"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseExceptionHandler("/Error");

            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSpaStaticFiles();

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TrackingHub>("/trackingHub");
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

                    if (env.IsDevelopment())
                    {
                        spa.UseReactDevelopmentServer(npmScript: "start");
                    }
                });
        }
    }
}
