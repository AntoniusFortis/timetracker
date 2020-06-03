
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Timetracker.Entities.Classes;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System;
using System.IO;

namespace Timetracker.Entities.Extensions
{
    public static class IServiceCollectionExtented
    {
        public static void AddDatabase( this IServiceCollection services, IConfiguration configuration )
        {
            services.AddDbContext<TimetrackerContext>( x =>
                 x.UseSqlServer( configuration.GetConnectionString( "Timetracker" ) ), contextLifetime: ServiceLifetime.Scoped );
        }

        public static void AddSwagger( this IServiceCollection services )
        {
            services.AddSwaggerGen( c =>
            {
                c.SwaggerDoc( "v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Time Tracker API"
                } );

                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments( xmlPath );
            } );
        }

        public static void AddAuth( this IServiceCollection services )
        {
            services.AddAuthorization();
            services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme )
               .AddJwtBearer( options =>
               {
                   options.SaveToken = true;
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
                           if ( !string.IsNullOrEmpty( accessToken ) && ( path.StartsWithSegments( "/trackingHub" ) ) )
                           {
                               context.Token = accessToken;
                           }
                           return System.Threading.Tasks.Task.CompletedTask;
                       }
                   };
               } );
        }
    }
}
