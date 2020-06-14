using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Entity;

namespace Timetracker.Models.Helpers
{
    public static class TokenHelpers
    {
        private static readonly TimeSpan tokenDurability =  TimeSpan.FromDays( 30 );

        public static async Task GenerateToken( string login, Token user, TimetrackerContext context, bool isNew = false )
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, login)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            var now = DateTime.UtcNow;
            var expiredIn = now.Add( tokenDurability );
            var jwt = new JwtSecurityToken(
                    issuer: TimetrackerAuthorizationOptions.ISSUER,
                    audience: TimetrackerAuthorizationOptions.AUDIENCE,
                    notBefore: now,
                    claims: claimsIdentity.Claims,
                    expires: expiredIn,
                    signingCredentials: new SigningCredentials(TimetrackerAuthorizationOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var access_token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");

            user.AccessToken = access_token;
            user.RefreshToken = refresh_token;
            user.TokenExpiredDate = expiredIn;

            if ( isNew )
            {
                await context.AddAsync( user );
            }

            context.SaveChanges();
        }
    }
}
