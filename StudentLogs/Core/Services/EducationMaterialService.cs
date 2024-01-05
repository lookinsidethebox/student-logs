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
		Task<IEnumerable<EducationMaterialListItemModel>> GetEducationMaterialsForUserAsync(int userId, SortType sortType, bool isAdmin);
		Task<EducationMaterialItemModel> GetEducationMaterialByIdAsync(int id, int userId, SortType sortType, bool isAdmin);
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

		public async Task<IEnumerable<EducationMaterialListItemModel>> GetEducationMaterialsForUserAsync(int userId, SortType sortType, bool isAdmin)
		{
			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var repo = new BaseRepository<EducationMaterial>(db);
				var materials = await repo.GetAsync();
				var query = materials.OrderBy(x => !x.IsFirst).ThenBy(x => x.IsFinal);
				var result = new List<EducationMaterialListItemModel>();

				var materialIdsWithLogs = await db.Set<Log>().AsNoTracking()
					.Where(x => x.UserId == userId)
					.Where(x => x.EducationMaterial.Type != EducationMaterialType.Survey
						|| x.Type == LogType.SurveyCompleted && x.EducationMaterial.Type == EducationMaterialType.Survey)
					.Select(x => x.EducationMaterialId)
					.Distinct()
					.ToListAsync();

				var list = query.ThenByDescending(x => x.Order)
					.Select(x => new EducationMaterial
					{
						Id = x.Id,
						Description = x.Description,
						Title = x.Title,
						Type = x.Type,
						FilePath = x.FilePath,
						SurveyId = x.SurveyId,
						IsFinal = x.IsFinal,
						IsFirst = x.IsFirst,
						IsRequireOtherMaterials = x.IsRequireOtherMaterials,
						IsOneTime = x.IsOneTime
					});

				var hasUnwatchedMaterials = list
					.Where(x => !x.IsRequireOtherMaterials && !materialIdsWithLogs.Contains(x.Id))
					.Count() > 0;

				var isFirst = true;
				var nextIsActive = false;

				foreach (var item in list)
				{
					var isVisited = materialIdsWithLogs.Where(x => x == item.Id).FirstOrDefault() != 0;
					var shouldBeDisabled = isVisited && item.IsOneTime || hasUnwatchedMaterials && item.IsRequireOtherMaterials;

					var model = new EducationMaterialListItemModel
					{
						Id = item.Id,
						Description = item.Description,
						IsActive = nextIsActive && !shouldBeDisabled || isAdmin || isFirst,
						Title = item.Title,
						Type = (int)item.Type,
						FilePath = item.FilePath,
						IsFinal = item.IsFinal,
						IsFirst = item.IsFirst,
						SurveyId = item.SurveyId
					};

					result.Add(model);
					nextIsActive = sortType == SortType.ByRandom || isVisited && (model.IsActive || shouldBeDisabled);

					if (isFirst)
						isFirst = false;
				}

				return result;
			}
		}

		public async Task<EducationMaterialItemModel> GetEducationMaterialByIdAsync(int id, int userId, SortType sortType, bool isAdmin)
		{
			var all = await GetEducationMaterialsForUserAsync(userId, sortType, isAdmin);

			if (!all.Where(x => x.Id == id && x.IsActive).Any() && !isAdmin)
				throw new AccessViolationException();

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
