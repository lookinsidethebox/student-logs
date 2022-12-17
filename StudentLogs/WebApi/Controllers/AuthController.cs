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
				var token = _authService.Login(email, password, out string role);

				if (string.IsNullOrEmpty(token))
					throw new Exception();

				var json = new
				{
					access_token = token,
					username = email,
					role = role
				};

				return new JsonResult(json);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return Unauthorized();
			}
		}
	}
}
