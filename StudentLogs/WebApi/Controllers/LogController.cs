using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("logs")]
	public class LogController : ControllerBase
	{
		private readonly ILogger<LogController> _logger;
		private readonly IAuthService _authService;
		private readonly ILogService _logService;

		public LogController(ILogger<LogController> logger,
			IAuthService authService,
			ILogService logService)
		{
			_logger = logger;
			_authService = authService;
			_logService = logService;
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAsync()
		{
			try
			{
				return Ok(await _logService.GetLogsAsync());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		[Route("byUserId")]
		public async Task<IActionResult> GetByUserAsync(int id)
		{
			try
			{
				return Ok(await _logService.GetLogsAsync(userId: id));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}

		[HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] LogItemModel data)
		{
			try
			{
				if (data.MaterialId == 0 || data.Type == 0)
					throw new Exception("Не заданы обязательные поля");

				var email = HttpContext.User.Identity?.Name;

				if (string.IsNullOrEmpty(email))
					throw new Exception("Не удалось получить Email пользователя");

				var user = _authService.GetCurrentUser(email);

				if (user == null)
					throw new Exception($"Пользователь с Email = {email} не найден");

				await _logService.CreateLog(data, user.Id);
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}
	}
}
