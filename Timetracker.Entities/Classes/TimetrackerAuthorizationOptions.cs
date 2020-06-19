/* Автор: Антон Другалев  
* Проект: Timetracker.View
*/

namespace Timetracker.Models.Classes
{
    using Microsoft.IdentityModel.Tokens;
    using System.Text;

    public class TimetrackerAuthorizationOptions
    {
        public const string ISSUER = "TimetrackerAuthorization";
        public const string AUDIENCE = "TimetrackerClient";
        public const string KEY = "mysupersecret_secretkeymysupersecret_secretkeymysupersecret_secretkeymysupersecret_secretkey!123";
        public const string KEYCrypt = "QlV2qv4fG/nT2nB01xz6siAZCdNuv0r30I1FBW2e1Ds=";

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}