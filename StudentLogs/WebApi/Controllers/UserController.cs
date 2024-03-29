﻿using Core.EF;
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
		private readonly ILogger<UserController> _logger;

		public UserController(IPasswordService passwordService,
			IDataContextOptionsHelper dataContextOptionsHelper,
			ILogger<UserController> logger)
		{
			_passwordService = passwordService;
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_logger = logger;
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
					return Ok(users
						.OrderBy(x => x.LastName)
						.Select(x => new UserModel
						{
							Id = x.Id,
							Email = x.Email,
							FirstName = x.FirstName,
							LastName = x.LastName,
							Role = (int)x.Role,
							RoleText = x.Role.ToString(),
							SortType = (int)x.SortType,
							SortText = x.SortType.ToString()
						}));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
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
						throw new Exception($"Пользователь с id = {id} не найден");

					return Ok(new UserModel
					{ 
						Id = user.Id,
						Email = user.Email,
						FirstName = user.FirstName,
						LastName = user.LastName,
						Role = (int)user.Role,
						RoleText = user.Role.ToString(),
						SortType = (int)user.SortType,
						SortText = user.Role.ToString()
					});
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] SaveUserModel data)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(data.Email))
					throw new Exception("Не задано поле Email");

				if (!StringHelper.IsValidEmail(data.Email))
					throw new Exception("Некорректное значение поля Email");

				if (string.IsNullOrWhiteSpace(data.Password))
					throw new Exception("Не задано поле Password");

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<User>(db);
					var user = repo.GetByPredicate(x => x.Email == data.Email).FirstOrDefault();

					if (user != null)
						throw new Exception($"Пользователь с Email = {data.Email} уже существует");

					var newUser = new User
					{
						FirstName = data.FirstName,
						LastName = data.LastName,
						Email = data.Email,
						PasswordHash = _passwordService.GenerateHash(data.Password),
						Role = (UserRole)data.Role,
						SortType = data.SortType.HasValue ? (SortType)data.SortType.Value : SortType.NotSet
					};

					newUser.Id = await repo.CreateAsync(newUser);
					return Ok(new UserModel
					{
						Id = newUser.Id,
						Email = newUser.Email,
						FirstName = newUser.FirstName,
						LastName = newUser.LastName,
						Role = (int)newUser.Role,
						RoleText = newUser.Role.ToString(),
						SortType = (int)newUser.SortType,
						SortText = newUser.Role.ToString()
					});
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpPost]
		[Route("password")]
		public async Task<IActionResult> ChangePasswordAsync([FromBody] PasswordModel data)
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<User>(db);
					var user = await repo.GetByIdAsync(data.UserId);

					if (user == null)
						throw new Exception($"Пользователь с id = {data.UserId} не найден");

					user.PasswordHash = _passwordService.GenerateHash(data.Password);
					await repo.UpdateAsync(user);
					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpPut]
		public async Task<IActionResult> PutAsync([FromBody] SaveUserModel data)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(data.Email))
					throw new Exception("Не задано поле Email");

				if (!StringHelper.IsValidEmail(data.Email))
					throw new Exception("Некорректное значение поля Email");

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<User>(db);
					var user = await repo.GetByIdAsync(data.Id);

					if (user == null)
						throw new Exception($"Пользователь с id = {data.Id} не найден");

					user.FirstName = data.FirstName;
					user.LastName = data.LastName;
					user.Email = data.Email;
					user.Role = (UserRole)data.Role;
					user.SortType = data.SortType.HasValue ? (SortType)data.SortType.Value : SortType.NotSet;
					await repo.UpdateAsync(user);
					return Ok(new UserModel
					{
						Id = user.Id,
						Email = user.Email,
						FirstName = user.FirstName,
						LastName = user.LastName,
						Role = (int)user.Role,
						RoleText = user.Role.ToString(),
						SortType = (int)user.SortType,
						SortText = user.Role.ToString()
					});
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
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
					var userAnswersRepo = new BaseRepository<UserAnswer>(db);
					var userAnswers = userAnswersRepo.GetByPredicate(x => x.UserId == id);

					foreach (var answer in userAnswers)
						await userAnswersRepo.DeleteAsync(answer.Id);

					var logRepo = new BaseRepository<Log>(db);
					var logs = logRepo.GetByPredicate(x => x.UserId == id);

					foreach (var log in logs)
						await logRepo.DeleteAsync(log.Id);

					var repo = new BaseRepository<User>(db);
					await repo.DeleteAsync(id);
					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}
	}
}
