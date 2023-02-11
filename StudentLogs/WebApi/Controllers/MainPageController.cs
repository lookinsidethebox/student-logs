using Core.Enums;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("main")]
	public class MainPageController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly ILogger<MainPageController> _logger;
		private readonly IEducationMaterialService _educationMaterialService;

		public MainPageController(IAuthService authService,
			ILogger<MainPageController> logger,
			IEducationMaterialService educationMaterialService)
		{
			_authService = authService;
			_logger = logger;
			_educationMaterialService = educationMaterialService;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			try
			{
				var email = HttpContext.User.Identity?.Name;

				if (string.IsNullOrEmpty(email))
					throw new UnauthorizedAccessException();

				var user = _authService.GetCurrentUser(email);

				if (user == null)
					throw new UnauthorizedAccessException();

				if (user.SortType == SortType.NotSet && user.Role != UserRole.Admin)
					throw new UnauthorizedAccessException();

				return Ok(await _educationMaterialService.GetEducationMaterialsForUserAsync(user.Id, user.SortType, user.Role == UserRole.Admin));
			}
			catch (UnauthorizedAccessException uae)
			{
				_logger.LogError(uae, uae.Message);
				return Unauthorized();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}

		[HttpGet]
		[Route("byId")]
		public async Task<IActionResult> GetById(int id)
		{
			try
			{
				var email = HttpContext.User.Identity?.Name;

				if (string.IsNullOrEmpty(email))
					throw new UnauthorizedAccessException();

				var user = _authService.GetCurrentUser(email);

				if (user == null)
					throw new UnauthorizedAccessException();

				if (user.SortType == SortType.NotSet && user.Role != UserRole.Admin)
					throw new UnauthorizedAccessException();

				return Ok(await _educationMaterialService.GetEducationMaterialByIdAsync(id, user.Id, user.SortType, user.Role == UserRole.Admin));
			}
			catch (UnauthorizedAccessException uae)
			{
				_logger.LogError(uae, uae.Message);
				return Unauthorized();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}
	}
}
