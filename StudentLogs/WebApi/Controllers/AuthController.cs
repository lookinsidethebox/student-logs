using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("auth")]
	[AllowAnonymous]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly ILogger<AuthController> _logger;

		public AuthController(IAuthService authService,
			ILogger<AuthController> logger)
		{
			_authService = authService;
			_logger = logger;
		}

		[HttpGet]
		[Route("login")]
		public IActionResult Login(string email, string password)
		{
			try
			{
				var model = _authService.Login(email, password);

				if (string.IsNullOrEmpty(model.Token))
					throw new Exception();

				var json = new
				{
					access_token = model.Token,
					username = email,
					role = model.Role,
					sort = model.Sort,
					id = model.Id
				};

				return Ok(json);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return Unauthorized();
			}
		}
	}
}
