using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
					var query = materials.OrderBy(x => !x.IsFirst).ThenBy(x => x.IsFinal);
					var result = new List<EducationMaterialListItemModel>();

					var logIds = await db.Set<Log>().AsNoTracking()
						.Where(x => x.EducationMaterial.Type != EducationMaterialType.Survey
							|| x.Type == LogType.SurveyCompleted && x.EducationMaterial.Type == EducationMaterialType.Survey)
						.Select(x => x.EducationMaterialId)
						.Distinct()
						.ToListAsync();

					if (user.SortType == SortType.ByTeacherChoice)
					{
						var list = query.ThenBy(x => x.Order)
							.Select(x => new EducationMaterial
							{ 
								Id = x.Id,
								Description = x.Description,
								Title = x.Title,
								Type = x.Type
							});

						var prevIsActive = true;

						foreach (var item in list)
						{
							var logExists = logIds.Where(x => x == item.Id).FirstOrDefault() != 0;

							var model = new EducationMaterialListItemModel
							{
								Id = item.Id,
								Description = item.Description,
								IsActive = prevIsActive && logExists,
								Title = item.Title,
								Type = (int)item.Type
							};

							result.Add(model);
							prevIsActive = model.IsActive;
						}

						return Ok(result);
					}
					else
					{
						var random = new Random();

						var list = query.ThenBy(x => random.Next())
							.Select(x => new EducationMaterial
							{
								Id = x.Id,
								Description = x.Description,
								Title = x.Title,
								Type = x.Type,
								IsFinal = x.IsFinal,
								IsFirst = x.IsFirst
							});

						var firstMaterialIds = list
							.Where(x => x.IsFirst)
							.Select(x => x.Id);

						var notFinalMaterialIds = list
							.Where(x => !x.IsFinal)
							.Select(x => x.Id);

						var allFirstMaterialsPassed = firstMaterialIds.All(x => logIds.Contains(x));
						var allNotFinalMaterialsPassed = notFinalMaterialIds.All(x => logIds.Contains(x));
						var isFirst = true;

						foreach (var item in list)
						{
							var isActive = isFirst
								|| item.IsFirst 
								|| allFirstMaterialsPassed && !item.IsFinal 
								|| allNotFinalMaterialsPassed && item.IsFinal;

							if (isFirst)
								isFirst = false;

							var model = new EducationMaterialListItemModel
							{
								Id = item.Id,
								Description = item.Description,
								IsActive = isActive,
								Title = item.Title,
								Type = (int)item.Type
							};

							result.Add(model);
						}

						return Ok(result);
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
