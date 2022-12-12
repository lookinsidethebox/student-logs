using Core.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Core.Services
{
	public interface IPasswordService
	{
		string GenerateHash(string password);
		bool PasswordIsValid(string password, string hash);
	}

	public class PasswordService : IPasswordService
	{
		private readonly IOptions<PasswordOptions> _passwordOptions;

		public PasswordService(IOptions<PasswordOptions> passwordOptions)
		{
			_passwordOptions = passwordOptions;
		}

		public string GenerateHash(string password)
		{
			if (_passwordOptions == null 
				|| _passwordOptions.Value == null 
				|| string.IsNullOrEmpty(_passwordOptions.Value.Salt))
				throw new Exception();

			var salt = Encoding.ASCII.GetBytes(_passwordOptions.Value.Salt);

			var hash = Rfc2898DeriveBytes.Pbkdf2(
				Encoding.UTF8.GetBytes(password),
				salt,
				_passwordOptions.Value.Iterations,
				HashAlgorithmName.SHA512,
				_passwordOptions.Value.KeySize);

			return Convert.ToHexString(hash);
		}

		public bool PasswordIsValid(string password, string hash)
		{ 
			var currentHash = GenerateHash(password);
			return currentHash.SequenceEqual(hash);
		}
	}
}
