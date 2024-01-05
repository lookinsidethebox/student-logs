using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using System.Text;

namespace Core.Services
{
	public interface IReportService
	{
		Task<byte[]> GenerateSummaryReportAsync();
		Task<byte[]> GenerateUserReportAsync(int userId);
		Task<string> GetReportName(int? userId = null);
	}

	public class ReportService : IReportService
	{
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;
		private readonly IEducationMaterialService _educationMaterialService;
		private readonly ILogService _logService;

		public ReportService(IDataContextOptionsHelper dataContextOptionsHelper,
			IEducationMaterialService educationMaterialService,
			ILogService logService)
		{
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_educationMaterialService = educationMaterialService;
			_logService = logService;
		}

		private const string CSV_EXTENSION = ".csv";

		public async Task<byte[]> GenerateSummaryReportAsync()
		{
			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var userRepo = new BaseRepository<User>(db);
				var logRepo = new BaseRepository<Log>(db);
				var surveyRepo = new BaseRepository<Survey>(db);
				var questionsRepo = new BaseRepository<Question>(db);
				var answersRepo = new BaseRepository<Answer>(db);
				var userAnswersRepo = new BaseRepository<UserAnswer>(db);

				var userId = (await userRepo.GetAsync()).First().Id;
				var materials = await _educationMaterialService.GetEducationMaterialsForUserAsync(userId, SortType.ByTeacherChoice, true);
				var students = userRepo.GetByPredicate(x => x.Role == UserRole.Student);
				var surveys = await surveyRepo.GetAsync();
				var questions = (await questionsRepo.GetAsync()).Where(x => !x.WithoutAnswers);
				var answers = (await answersRepo.GetAsync()).ToDictionary(x => x.Id, x => x.Value);

				var surveyData = surveys.Select(x => new
					{
						Id = x.Id,
						Title = x.Title,
						Questions = questions.Where(q => q.SurveyId == x.Id).Select(q => new
						{
							Id = q.Id,
							Title = q.Value
						})
						.OrderBy(q => q.Id).ToList()
					})
					.ToDictionary(x => x.Id, x => x);

				var userAnswers = (await userAnswersRepo.GetAsync())
					.ToDictionary(x => (x.UserId, x.QuestionId), x => (x.AnswerId, x.TextAnswer));

				var sb = new StringBuilder();
				var header = "Студент;Логин;";

				foreach (var material in materials)
				{
					if ((EducationMaterialType)material.Type == EducationMaterialType.Survey)
					{
						if (!material.SurveyId.HasValue)
							continue;

						var hasSurveyInfo = surveyData.TryGetValue(material.SurveyId.Value, out var survey);

						if (!hasSurveyInfo || survey == null)
							continue;

						foreach (var question in survey.Questions)
							header += $"{question.Title};";
					}
					else
						header += $"{material.Title};";
				}

				sb.AppendLine(header.TrimEnd(';'));

				foreach (var student in students)
				{
					var logs = logRepo.GetByPredicate(x => x.UserId == student.Id);
					var studentRow = $"{student.LastName} {student.FirstName};{student.Email};";

					foreach (var material in materials)
					{
						var type = (EducationMaterialType)material.Type;
						var materialLogs = logs.Where(x => x.EducationMaterialId == material.Id);

						switch (type)
						{
							case EducationMaterialType.Video:
								var videoLogs = materialLogs
									.Where(x => x.Type == LogType.VideoStarted || x.Type == LogType.VideoStopped)
									.OrderBy(x => x.Id);

								double seconds = 0;
								DateTime? startDate = null;

								foreach (var videoLog in videoLogs)
								{
									if (videoLog.Type == LogType.VideoStarted)
									{
										startDate = videoLog.CreateDate;
										continue;
									}

									if (!startDate.HasValue)
										continue;

									seconds += (videoLog.CreateDate - startDate.Value).TotalSeconds;
								}

								studentRow += $"Просмотрено: {GetWatchTime(seconds)};";
								break;
							case EducationMaterialType.Document:
								var downloadCount = materialLogs.Where(x => x.Type == LogType.Downloaded).Count();
								studentRow += $"Кол-во скачиваний: {downloadCount};";
								break;
							case EducationMaterialType.Survey:
								if (!material.SurveyId.HasValue)
									break;

								var hasSurveyInfo = surveyData.TryGetValue(material.SurveyId.Value, out var survey);

								if (!hasSurveyInfo || survey == null)
									break;

								foreach (var question in survey.Questions)
								{
									var hasAnswer = userAnswers.TryGetValue((student.Id, question.Id), out var userAnswer);

									if (hasAnswer)
									{
										var answer = userAnswer.AnswerId.HasValue && answers.TryGetValue(userAnswer.AnswerId.Value, out var answerValue)
											? answerValue
											: userAnswer.TextAnswer;
										studentRow += $"{answer};";
									}
									else
										studentRow += ";";
								}
								break;
							case EducationMaterialType.Text:
								var clickCount = materialLogs.Where(x => x.Type == LogType.Clicked).Count();
								studentRow += $"Кол-во переходов: {clickCount};";
								break;
						}
					}

					sb.AppendLine(studentRow.TrimEnd(';'));
				}

				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				return Encoding.GetEncoding(1251).GetBytes(sb.ToString());
			}
		}

		private string GetWatchTime(double seconds)
		{
			if (seconds < 60)
				return $"{seconds} сек.";

			var minutes = (int)Math.Floor(seconds / 60);

			if (seconds < 60 * 60)
				return $"{minutes} мин. {seconds - minutes * 60} сек.";

			var hours = (int)Math.Floor(seconds / (60 * 60));
			var minutesLast = minutes - hours * 60;
			return $"{hours} ч. {minutesLast} мин. {seconds - minutesLast * 60 - hours * 60 * 60} сек.";
		}

		public async Task<byte[]> GenerateUserReportAsync(int userId)
		{
			var logs = await _logService.GetLogsAsync(userId: userId);
			var sb = new StringBuilder();
			sb.AppendLine("Студент;Дата;Обучающий материал;Тип действия;Комментарий");

			foreach (var log in logs)
				sb.AppendLine($"{log.User};{log.CreateDate};{log.EducationMaterial};{log.Type};{log.Info}");

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			return Encoding.GetEncoding(1251).GetBytes(sb.ToString());
		}

		public async Task<string> GetReportName(int? userId = null)
		{
			var date = DateTime.Now.ToLocalTime().ToString("dd-MM-yyyy_HH-mm-ss");

			if (!userId.HasValue)
				return $"summary_report_{date}{CSV_EXTENSION}";

			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var userRepo = new BaseRepository<User>(db);
				var user = await userRepo.GetByIdAsync(userId.Value);
				return $"user_report_{user.LastName}_{user.FirstName}_{date}{CSV_EXTENSION}";
			}
		}
	}
}
