using Core.EF;
using Core.Entities;
using Core.Helpers;

namespace Core.Services
{
	public interface ILogService
	{
		Task<IEnumerable<Log>> GetLogsAsync(int? materialId = null, int? userId = null);
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
	}
}
