using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Timetracker.Entities.Classes
{
    public class TimetrackerAuthorizationOptions
    {
        public const string ISSUER = "TimetrackerAuthorization";
        public const string AUDIENCE = "TimetrackerClient";
        public const string KEY = "mysupersecret_secretkeymysupersecret_secretkeymysupersecret_secretkeymysupersecret_secretkey!123";
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
