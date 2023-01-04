using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
	public interface IEducationMaterialService
	{
		Task<IEnumerable<EducationMaterialListItemModel>> GetEducationMaterialsForUserAsync(int userId, SortType sortType);
		Task<EducationMaterialItemModel> GetEducationMaterialByIdAsync(int id, int userId, SortType sortType);
	}

	public class EducationMaterialService : IEducationMaterialService
	{
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;
		private readonly ILogService _logService;

		public EducationMaterialService(IDataContextOptionsHelper dataContextOptionsHelper,
			ILogService logService)
		{
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_logService = logService;
		}

		public async Task<IEnumerable<EducationMaterialListItemModel>> GetEducationMaterialsForUserAsync(int userId, SortType sortType)
		{
			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var repo = new BaseRepository<EducationMaterial>(db);
				var materials = await repo.GetAsync();
				var query = materials.OrderBy(x => !x.IsFirst).ThenBy(x => x.IsFinal);
				var result = new List<EducationMaterialListItemModel>();

				var logIds = await db.Set<Log>().AsNoTracking()
					.Where(x => x.UserId == userId)
					.Where(x => x.EducationMaterial.Type != EducationMaterialType.Survey
						|| x.Type == LogType.SurveyCompleted && x.EducationMaterial.Type == EducationMaterialType.Survey)
					.Select(x => x.EducationMaterialId)
					.Distinct()
					.ToListAsync();

				if (sortType == SortType.ByTeacherChoice)
				{
					var list = query.ThenBy(x => x.Order)
						.Select(x => new EducationMaterial
						{
							Id = x.Id,
							Description = x.Description,
							Title = x.Title,
							Type = x.Type,
							FilePath = x.FilePath
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
							Type = (int)item.Type,
							FilePath = item.FilePath
						};

						result.Add(model);
						prevIsActive = model.IsActive;
					}

					return result;
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
							IsFirst = x.IsFirst,
							FilePath = x.FilePath
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
							Type = (int)item.Type,
							FilePath = item.FilePath
						};

						result.Add(model);
					}

					return result;
				}
			}
		}

		public async Task<EducationMaterialItemModel> GetEducationMaterialByIdAsync(int id, int userId, SortType sortType)
		{
			var all = await GetEducationMaterialsForUserAsync(userId, sortType);

			if (!all.Where(x => x.Id == id && x.IsActive).Any())
				throw new UnauthorizedAccessException();

			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var repo = new BaseRepository<EducationMaterial>(db);
				var material = await repo.GetByIdAsync(id);

				await _logService.CreateLog(new LogItemModel
				{
					MaterialId = id,
					Type = (int)LogType.Clicked
				}, userId);

				return new EducationMaterialItemModel
				{
					Id = id,
					AnswerCount = material.AnswersCount,
					ClickCount = ++material.PageClickCount,
					PlayCount = material.PlayStartCount,
					Description = material.Description,
					FilePath = material.FilePath,
					SurveyId = material.SurveyId,
					Text = material.Text,
					Title = material.Title,
					Type = (int)material.Type
				};
			}
		}
	}
}
