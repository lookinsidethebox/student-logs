using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("user")]
	public class UserController : ControllerBase
	{
		private readonly IPasswordService _passwordService;

		public UserController(IPasswordService passwordService)
		{
			_passwordService = passwordService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAsync()
		{
			try
			{
				using (var db = new DataContext())
				{
					var repo = new BaseRepository<User>(db);
					var users = await repo.GetAsync();
					return Ok(users);
				}
			}
			catch
			{
				return BadRequest();
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetAsync(int id)
		{
			try
			{
				using (var db = new DataContext())
				{
					var repo = new BaseRepository<User>(db);
					var user = await repo.GetByIdAsync(id);

					if (user == null)
						throw new Exception();

					return Ok(user);
				}
			}
			catch
			{
				return BadRequest();
			}
		}

		[HttpPost]
		public async Task<IActionResult> PostAsync(UserModel data)
		{
			try
			{
				using (var db = new DataContext())
				{
					var user = new User
					{
						FirstName = data.FirstName,
						LastName = data.LastName,
						Email = data.Email,
						PasswordHash = _passwordService.GenerateHash(data.Password),
						Role = (UserRole)data.Role,
						SortType = data.SortType.HasValue ? (SortType)data.SortType.Value : SortType.NotSet
					};

					var repo = new BaseRepository<User>(db);
					await repo.CreateAsync(user);
					return Ok();
				}
			}
			catch
			{
				return BadRequest();
			}
		}

		[HttpPost]
		[Route("password")]
		public async Task<IActionResult> ChangePasswordAsync(PasswordModel data)
		{
			try
			{
				using (var db = new DataContext())
				{
					var repo = new BaseRepository<User>(db);
					var user = await repo.GetByIdAsync(data.UserId);

					if (user == null)
						throw new Exception();

					user.PasswordHash = _passwordService.GenerateHash(data.Password);
					await repo.UpdateAsync(user);
					return Ok();
				}
			}
			catch
			{
				return BadRequest();
			}
		}

		[HttpPut]
		public async Task<IActionResult> PutAsync(UserModel data)
		{
			try
			{
				using (var db = new DataContext())
				{
					var repo = new BaseRepository<User>(db);
					var user = await repo.GetByIdAsync(data.Id);

					if (user == null)
						throw new Exception();

					user.FirstName = data.FirstName;
					user.LastName = data.LastName;
					user.Email = data.Email;
					user.Role = (UserRole)data.Role;
					user.SortType = data.SortType.HasValue ? (SortType)data.SortType.Value : SortType.NotSet;
					await repo.UpdateAsync(user);
					return Ok();
				}
			}
			catch
			{
				return BadRequest();
			}
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteAsync(int id)
		{
			try
			{
				using (var db = new DataContext())
				{
					var repo = new BaseRepository<User>(db);
					await repo.DeleteAsync(id);
					return Ok();
				}
			}
			catch
			{
				return BadRequest();
			}
		}
	}
}
