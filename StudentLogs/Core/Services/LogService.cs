using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
	public interface ILogService
	{
		Task<IEnumerable<LogModel>> GetLogsAsync(int? materialId = null, int? userId = null);
		Task CreateLog(LogItemModel data, int userId);
	}

	public class LogService : ILogService
	{
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;

		public LogService(IDataContextOptionsHelper dataContextOptionsHelper)
		{
			_dataContextOptionsHelper = dataContextOptionsHelper;
		}

		public async Task<IEnumerable<LogModel>> GetLogsAsync(int? materialId = null, int? userId = null)
		{
			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var query = db.Set<Log>().AsNoTracking();

				if (materialId.HasValue && userId.HasValue)
					query = query.Where(x =>
						x.EducationMaterialId == materialId.Value && x.UserId == userId.Value);
				else if (materialId.HasValue)
					query = query.Where(x => x.EducationMaterialId == materialId.Value);
				else if (userId.HasValue)
					query = query.Where(x => x.UserId == userId.Value);

				return await query
					.Select(x => new LogModel
					{ 
						CreateDate = x.CreateDate.ToString("dd.MM.yyyy HH:mm:ss"),
						EducationMaterial = x.EducationMaterial.Title,
						Type = x.Type.GetStringValue(),
						User = x.User.Title,
						UserId = x.User.Id
					})
					.OrderByDescending(x => x.CreateDate)
					.ToListAsync();
			}
		}

		public async Task CreateLog(LogItemModel data, int userId)
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
