using Core.EF;
using Core.Entities;
using Core.Helpers;
using Core.Models;
using Core.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core.Services
{
	public interface IAuthService
	{
		AuthModel Login(string email, string password);
		User GetCurrentUser(string email);
	}

	public class AuthService : IAuthService
	{
		private readonly IPasswordService _passwordService;
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;

		public AuthService(IPasswordService passwordService,
			IDataContextOptionsHelper dataContextOptionsHelper)
		{
			_passwordService = passwordService;
			_dataContextOptionsHelper = dataContextOptionsHelper;
		}

		public AuthModel Login(string email, string password)
		{
			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var repo = new BaseRepository<User>(db);
				var user = repo.GetByPredicate(x => x.Email == email).FirstOrDefault();

				if (user == null)
					throw new Exception();

				var passwordIsValid = _passwordService.PasswordIsValid(password, user.PasswordHash);

				if (!passwordIsValid)
					throw new Exception();

				return new AuthModel
				{ 
					Id = user.Id,
					Role = (int)user.Role,
					Sort = (int)user.SortType,
					Token = GenerateToken(email, user.Role.ToString())
				};
			}
		}

		public User GetCurrentUser(string email)
		{
			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var repo = new BaseRepository<User>(db);
				var user = repo.GetByPredicate(x => x.Email == email).FirstOrDefault();

				if (user == null)
					throw new Exception();

				return user;
			}
		}

		private string GenerateToken(string email, string role)
		{
			var claims = new List<Claim> {
				new Claim(ClaimTypes.Name, email),
				new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
			};

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
