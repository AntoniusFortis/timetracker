using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Timetracker.Models.Classes;
using Timetracker.Models.Entities;

namespace Timetracker.Models.Helpers
{
    public static class TokenHelpers
    {
        private static readonly TimeSpan tokenDurability =  TimeSpan.FromDays( 28 );

        public static async Task GenerateToken( string id, Token user, TimetrackerContext context, bool isNew = false )
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, id)
            };

            var claimsIdentity = new ClaimsIdentity( claims, "Token", 
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType );

            var now = DateTime.UtcNow;
            var expiredIn = now.Add( tokenDurability );
            var jwt = new JwtSecurityToken(
                    issuer: TimetrackerAuthorizationOptions.ISSUER,
                    audience: TimetrackerAuthorizationOptions.AUDIENCE,
                    notBefore: now,
                    claims: claimsIdentity.Claims,
                    expires: expiredIn,
                    signingCredentials: new SigningCredentials(
                        TimetrackerAuthorizationOptions.GetSymmetricSecurityKey(), 
                        SecurityAlgorithms.HmacSha256 )
                    );

            var access_token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");

            user.AccessToken = access_token;
            user.RefreshToken = refresh_token;
            user.TokenExpiredDate = expiredIn;

            if ( isNew )
            {
                await context.AddAsync( user )
                    .ConfigureAwait( false );
            }

            await context.SaveChangesAsync( true )
                .ConfigureAwait( false );
        }
    }
}
