using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Core.Options
{
	public class IdentityOptions
	{
		public const string ISSUER = "StudentLogsIdentityServer";
		public const string AUDIENCE = "StudentLogsIdentityClient";
		const string KEY = "2hLT4yYjQahRZ4s72PRnnM7K";
		public const int HOURS = 24;
		public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
	}
}
