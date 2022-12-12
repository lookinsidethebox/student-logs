using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApi
{
    public class AuthOptions
    {
        public const string ISSUER = "StudentLogsIdentityServer";
        public const string AUDIENCE = "StudentLogsIdentityClient";
        const string KEY = "2hLT4yYjQahRZ4s72PRnnM7K";
        public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
