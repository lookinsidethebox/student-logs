using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("log")]
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
		public async Task<IActionResult> GetAsync(int? materialId = null, int? userId = null)
		{
			try
			{
				var logs = await _logService.GetLogsAsync(materialId, userId);
				return Ok(logs);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}

		[HttpPost]
		public async Task<IActionResult> PostAsync([FromForm] LogModel data)
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
