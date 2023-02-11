using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Options;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("file")]
	[AllowAnonymous]
	public class FileController : ControllerBase
	{
		private readonly ILogService _logService;
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;
		private readonly IAuthService _authService;
		private readonly ILogger<FileController> _logger;

		public FileController(ILogService logService,
			IDataContextOptionsHelper dataContextOptionsHelper,
			IAuthService authService,
			ILogger<FileController> logger)
		{
			_logService = logService;
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_authService = authService;
			_logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> GetFile(int id, string? token = null)
		{
			try
			{
				string email;

				if (string.IsNullOrEmpty(token))
					email = HttpContext.User.Identity?.Name;
				else
				{
					var validationParameters = new TokenValidationParameters
					{
						ValidAudience = IdentityOptions.AUDIENCE,
						ValidIssuer = IdentityOptions.ISSUER,
						RequireExpirationTime = true,
						SignatureValidator = delegate (string token, TokenValidationParameters parameters)
						{
							var jwt = new JwtSecurityToken(token);
							return jwt;
						}
					};

					var claim = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var lol);

					if (claim == null || claim.Identities == null || claim.Identities.Count() == 0)
						throw new UnauthorizedAccessException();

					email = claim.Identities.First().Name;
				}

				if (string.IsNullOrEmpty(email))
					throw new UnauthorizedAccessException();

				var user = _authService.GetCurrentUser(email);

				if (user == null)
					throw new UnauthorizedAccessException();

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<EducationMaterial>(db);
					var material = await repo.GetByIdAsync(id);

					if (material == null)
						throw new Exception($"Учебный материал с id = {id} не найден");

					if (string.IsNullOrEmpty(material.FilePath) || string.IsNullOrEmpty(material.FileContentType))
						throw new Exception($"Файл не найден");

					await _logService.CreateLog(new LogItemModel
					{
						MaterialId = id,
						Type = (int)LogType.Downloaded
					}, user.Id);

					var file = System.IO.File.ReadAllBytes(material.FilePath);
					return new FileContentResult(file, material.FileContentType);
				}
			}
			catch (UnauthorizedAccessException uae)
			{
				_logger.LogError(uae, uae.Message);
				return Unauthorized();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}
	}
}
