using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Models;

namespace Core.Services
{
	public interface ILogService
	{
		Task<IEnumerable<Log>> GetLogsAsync(int? materialId = null, int? userId = null);
		Task CreateLog(LogModel data, int userId);
	}

	public class LogService : ILogService
	{
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;

		public LogService(IDataContextOptionsHelper dataContextOptionsHelper)
		{
			_dataContextOptionsHelper = dataContextOptionsHelper;
		}

		public async Task<IEnumerable<Log>> GetLogsAsync(int? materialId = null, int? userId = null)
		{
			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var repo = new BaseRepository<Log>(db);
				IEnumerable<Log> logs;

				if (!materialId.HasValue && !userId.HasValue)
					logs = await repo.GetAsync();
				else if (materialId.HasValue && userId.HasValue)
					logs = repo.GetByPredicate(x =>
						x.EducationMaterialId == materialId.Value && x.UserId == userId.Value);
				else if (materialId.HasValue)
					logs = repo.GetByPredicate(x => x.EducationMaterialId == materialId.Value);
				else
					logs = repo.GetByPredicate(x => x.UserId == userId.Value);

				return logs;
			}
		}

		public async Task CreateLog(LogModel data, int userId)
		{
			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var log = new Log
				{
					CreateDate = DateTime.Now,
					EducationMaterialId = data.MaterialId,
					Type = (LogType)data.Type,
					Info = data.Info,
					UserId = userId
				};

				var logRepo = new BaseRepository<Log>(db);
				await logRepo.CreateAsync(log);

				var repo = new BaseRepository<EducationMaterial>(db);
				var material = await repo.GetByIdAsync(data.MaterialId);
				var needToSave = false;

				switch (log.Type)
				{
					case LogType.Clicked:
						material.PageClickCount += 1;
						needToSave = true;
						break;
					case LogType.VideoStarted:
						material.PlayStartCount += 1;
						needToSave = true;
						break;
					case LogType.SurveyCompleted:
						material.AnswersCount += 1;
						needToSave = true;
						break;
				}

				if (needToSave)
					await repo.UpdateAsync(material);
			}
		}
	}
}
