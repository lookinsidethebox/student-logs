using Core.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core.Services
{
	public interface IAuthService
	{
		string Login(string email, string password);
	}

	public class AuthService : IAuthService
	{
		public string Login(string email, string password)
		{
			//TODO: add password check
			return GenerateToken(email);
		}

		private string GenerateToken(string email)
		{
			var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };

			var jwt = new JwtSecurityToken(
					issuer: IdentityOptions.ISSUER,
					audience: IdentityOptions.AUDIENCE,
					claims: claims,
					expires: DateTime.UtcNow.Add(TimeSpan.FromHours(IdentityOptions.HOURS)),
					signingCredentials: new SigningCredentials(IdentityOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

			return new JwtSecurityTokenHandler().WriteToken(jwt);
		}
	}
}
