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

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpGet]
		[Route("login")]
		public IActionResult Login(string email, string password)
		{
			try
			{
				var token = _authService.Login(email, password);

				var json = new
				{
					access_token = token,
					username = email
				};

				return new JsonResult(json);
			}
			catch
			{
				return Unauthorized();
			}
		}
	}
}
