using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("user")]
	[Authorize(Roles = "Admin")]
	public class UserController : ControllerBase
	{
		private readonly IPasswordService _passwordService;
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;

		public UserController(IPasswordService passwordService,
			IDataContextOptionsHelper dataContextOptionsHelper)
		{
			_passwordService = passwordService;
			_dataContextOptionsHelper = dataContextOptionsHelper;
		}

		[HttpGet]
		public async Task<IActionResult> GetAsync()
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
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
		[Route("byId")]
		public async Task<IActionResult> GetByIdAsync(int id)
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
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
		public async Task<IActionResult> PostAsync([FromForm] UserModel data)
		{
			try
			{
				if (string.IsNullOrEmpty(data.Email))
					throw new Exception();

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<User>(db);
					var user = repo.GetByPredicate(x => x.Email == data.Email).FirstOrDefault();

					if (user != null)
						throw new Exception();

					var newUser = new User
					{
						FirstName = data.FirstName,
						LastName = data.LastName,
						Email = data.Email,
						PasswordHash = _passwordService.GenerateHash(data.Password),
						Role = (UserRole)data.Role,
						SortType = data.SortType.HasValue ? (SortType)data.SortType.Value : SortType.NotSet
					};

					await repo.CreateAsync(newUser);
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
		public async Task<IActionResult> ChangePasswordAsync([FromForm] PasswordModel data)
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
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
		public async Task<IActionResult> PutAsync([FromForm] UserModel data)
		{
			try
			{
				if (string.IsNullOrEmpty(data.Email))
					throw new Exception();

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
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
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
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
