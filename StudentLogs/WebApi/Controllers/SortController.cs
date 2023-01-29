using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("sort")]
	public class SortController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;
		private readonly ILogger<SortController> _logger;

		public SortController(IAuthService authService,
			IDataContextOptionsHelper dataContextOptionsHelper,
			ILogger<SortController> logger)
		{
			_authService = authService;
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_logger = logger;
		}

		[HttpGet]
		public IActionResult Get()
		{
			try
			{
				var email = HttpContext.User.Identity?.Name;

				if (string.IsNullOrEmpty(email))
					throw new Exception("Не удалось получить Email пользователя");

				var user = _authService.GetCurrentUser(email);

				if (user == null)
					throw new Exception($"Пользователь с Email = {email} не найден");

				return Ok((int)user.SortType);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return Unauthorized();
			}
		}

		[HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] SortRequestModel model)
		{
			try
			{
				var email = HttpContext.User.Identity?.Name;

				if (string.IsNullOrEmpty(email))
					throw new UnauthorizedAccessException();

				var user = _authService.GetCurrentUser(email);

				if (user == null)
					throw new UnauthorizedAccessException();

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<User>(db);
					user.SortType = (SortType)model.SortType;
					await repo.UpdateAsync(user);
					return Ok();
				}
			}
			catch (UnauthorizedAccessException uae)
			{
				_logger.LogError(uae, uae.Message);
				return Unauthorized();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}
	}
}
