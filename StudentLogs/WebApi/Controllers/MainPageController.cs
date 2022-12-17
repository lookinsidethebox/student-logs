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
	[Route("main")]
	public class MainPageController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;
		private readonly ILogger<MainPageController> _logger;

		public MainPageController(IAuthService authService,
			IDataContextOptionsHelper dataContextOptionsHelper,
			ILogger<MainPageController> logger)
		{
			_authService = authService;
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_logger = logger;
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

				if (user.SortType == SortType.NotSet)
					throw new UnauthorizedAccessException();

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<EducationMaterial>(db);
					var materials = await repo.GetAsync();

					if (user.SortType == SortType.ByTeacherChoice)
					{
						return Ok(materials.OrderBy(x => x.IsFirst)
							.ThenBy(x => !x.IsFinal)
							.ThenBy(x => x.Order)
							.Select(x => new EducationMaterialListItemModel
							{ 
								Id = x.Id,
								Description = x.Description,
								IsActive = true,
								Title = x.Title,
								Type = (int)x.Type
							}));
					}
					else
					{
						var random = new Random();

						return Ok(materials.OrderBy(x => x.IsFirst)
							.ThenBy(x => !x.IsFinal)
							.ThenBy(x => random.Next())
							.Select(x => new EducationMaterialListItemModel
							{
								Id = x.Id,
								Description = x.Description,
								IsActive = true,
								Title = x.Title,
								Type = (int)x.Type
							}));
					}
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
