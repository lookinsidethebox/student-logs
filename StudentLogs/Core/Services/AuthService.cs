using Core.EF;
using Core.Entities;
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
		private readonly IPasswordService _passwordService;

		public AuthService(IPasswordService passwordService)
		{
			_passwordService = passwordService;
		}

		public string Login(string email, string password)
		{
			using (var db = new DataContext())
			{
				var repo = new BaseRepository<User>(db);
				var user = repo.GetByPredicate(x => x.Email == email).FirstOrDefault();

				if (user == null)
					throw new Exception();

				var passwordIsValid = _passwordService.PasswordIsValid(password, user.PasswordHash);

				if (!passwordIsValid)
					throw new Exception();
			}

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
