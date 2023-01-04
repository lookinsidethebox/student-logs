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
	[Route("log")]
	public class LogController : ControllerBase
	{
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;
		private readonly ILogger<LogController> _logger;
		private readonly IAuthService _authService;
		private readonly ILogService _logService;

		public LogController(IDataContextOptionsHelper dataContextOptionsHelper,
			ILogger<LogController> logger,
			IAuthService authService, ILogService logService)
		{
			_dataContextOptionsHelper = dataContextOptionsHelper;
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

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var log = new Log
					{
						CreateDate = DateTime.Now,
						EducationMaterialId = data.MaterialId,
						Type = (LogType)data.Type,
						Info = data.Info,
						UserId = user.Id
					};

					var repo = new BaseRepository<Log>(db);
					await repo.CreateAsync(log);
					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}
	}
}
