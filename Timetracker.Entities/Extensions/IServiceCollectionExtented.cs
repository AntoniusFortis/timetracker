
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Timetracker.Entities.Classes;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Threading.Tasks;

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
                       ValidateIssuer = false,
                       ValidIssuer = TimetrackerAuthorizationOptions.ISSUER,

                       ValidateAudience = true,
                       ValidAudience = TimetrackerAuthorizationOptions.AUDIENCE,

                       //ValidateLifetime = true,
                       ValidateLifetime = false,

                       IssuerSigningKey = TimetrackerAuthorizationOptions.GetSymmetricSecurityKey(),
                       ValidateIssuerSigningKey = true,

                       ClockSkew = TimeSpan.Zero
                   };
                   options.Events = new JwtBearerEvents
                   {
                       OnTokenValidated = ctx =>
                       {
                           var now = DateTime.UtcNow;
                           var path = ctx.HttpContext.Request.Path;

                           if ( now > ctx.SecurityToken.ValidTo && !path.StartsWithSegments( "/trackingHub" ) )
                           {
                               ctx.Fail( "Token expired" );
                           }
                           return Task.CompletedTask;
                       },
                       OnMessageReceived = context =>
                       {
                           var accessToken = context.Request.Query["access_token"];
                           var path = context.HttpContext.Request.Path;
                           if ( !string.IsNullOrEmpty( accessToken ) && path.StartsWithSegments( "/trackingHub" ) )
                           {
                               context.Token = accessToken;
                           }
                           return Task.CompletedTask;
                       }
                   };
               } );
        }
    }
}
