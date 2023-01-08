using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("survey")]
	public class SurveyController : ControllerBase
	{
		private readonly ILogger<SurveyController> _logger;
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;
		private readonly IAuthService _authService;
		private readonly ILogService _logService;

		public SurveyController(ILogger<SurveyController> logger,
			IDataContextOptionsHelper dataContextOptionsHelper,
			IAuthService authService,
			ILogService logService)
		{
			_logger = logger;
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_authService = authService;
			_logService = logService;
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAsync()
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<Survey>(db);
					var surveys = await repo.GetAsync();
					return Ok(surveys.OrderBy(x => x.Title));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpGet]
		[Route("byId")]
		public async Task<IActionResult> GetByIdAsync(int id)
		{
			try
			{
				var email = HttpContext.User.Identity?.Name;

				if (string.IsNullOrEmpty(email))
					throw new Exception("Не удалось получить Email пользователя");

				var user = _authService.GetCurrentUser(email);

				if (user == null)
					throw new Exception($"Пользователь с Email = {email} не найден");

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var surveyRepo = new BaseRepository<Survey>(db);
					var survey = await surveyRepo.GetByIdAsync(id);

					var questionRepo = new BaseRepository<Question>(db);
					var questions = questionRepo.GetByPredicate(x => x.SurveyId == id);
					var questionModels = new List<QuestionModel>();

					var answerRepo = new BaseRepository<Answer>(db);
					var answers = answerRepo.GetByPredicate(x => questions.Select(x => x.Id).Contains(x.QuestionId))
						.GroupBy(x => x.QuestionId)
						.Select(x => new
						{
							x.Key,
							Answers = x.Where(c => c.QuestionId == x.Key).ToList()
						})
						.ToDictionary(x => x.Key, x => x.Answers);

					var userAnswerRepo = new BaseRepository<UserAnswer>(db);
					var userAnswers = userAnswerRepo.GetByPredicate(x => x.UserId == user.Id && x.SurveyId == id);
					var isCompleted = userAnswers.Any();
					var groupped = new Dictionary<int, (int?, string?)>();

					if (isCompleted)
						groupped = userAnswers.GroupBy(x => x.QuestionId)
							.Select(x => new
							{
								x.Key,
								AnswerId = x.Where(c => c.QuestionId == x.Key).FirstOrDefault()?.AnswerId,
								TextAnswer = x.Where(c => c.QuestionId == x.Key).FirstOrDefault()?.TextAnswer
							})
							.ToDictionary(x => x.Key, x => (x.AnswerId, x.TextAnswer));

					var random = new Random();

					foreach (var q in questions)
					{
						(int?, string?) answer = (null, null);
						var hasUserAnswer = isCompleted && groupped.TryGetValue(q.Id, out answer);

						var model = new QuestionModel
						{
							Id = q.Id,
							Title = q.Value,
							Value = hasUserAnswer ? answer.Item2 : null,
							Answers = q.HasAnswers && answers.TryGetValue(q.Id, out var ans) 
								? ans.Select(x => new AnswerModel
									{ 
										Id = x.Id,
										Title = x.Value,
										IsSelected = hasUserAnswer ? answer.Item1 == x.Id : false
									}).OrderBy(x => random.Next())
								: null
						};

						questionModels.Add(model);
					}

					var result = new SurveyModel
					{
						Id = survey.Id,
						Title = survey.Title,
						IsCompleted = isCompleted,
						Questions = questionModels.OrderBy(x => random.Next())
					};

					return Ok(result);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> PostAsync([FromBody] SaveSurveyModel data)
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var surveyRepo = new BaseRepository<Survey>(db);
					var survey = new Survey { Title = data.Title };
					var surveyId = await surveyRepo.CreateAsync(survey);
					var questionRepo = new BaseRepository<Question>(db);
					var answerRepo = new BaseRepository<Answer>(db);

					foreach (var q in data.Questions)
					{
						var hasAnswers = q.Answers != null && q.Answers.Count > 0;

						var question = new Question
						{
							SurveyId = surveyId,
							Value = q.Title,
							HasAnswers = hasAnswers
						};

						var questionId = await questionRepo.CreateAsync(question);

						if (hasAnswers)
						{
							foreach (var a in q.Answers)
							{
								var answer = new Answer
								{
									QuestionId = questionId,
									Value = a
								};

								await answerRepo.CreateAsync(answer);
							}
						}
					}

					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpPost]
		[Route("complete")]
		public async Task<IActionResult> CompleteSurveyAsync([FromBody] CompleteSurveyModel data)
		{
			try
			{
				var email = HttpContext.User.Identity?.Name;

				if (string.IsNullOrEmpty(email))
					throw new Exception("Не удалось получить Email пользователя");

				var user = _authService.GetCurrentUser(email);

				if (user == null)
					throw new Exception($"Пользователь с Email = {email} не найден");

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var answerRepo = new BaseRepository<UserAnswer>(db);
					var answered = answerRepo.GetByPredicate(x => x.SurveyId == data.SurveyId && x.UserId == user.Id).Any();

					if (answered)
						throw new Exception("Этот пользователь уже заполнял данный опрос");

					var questionIds = data.Answers.Select(x => x.QuestionId).Distinct().ToList();
					var questionRepo = new BaseRepository<Question>(db);
					var questions = questionRepo.GetByPredicate(x => x.SurveyId == data.SurveyId);

					if (questions.Select(x => x.Id).Any(x => !questionIds.Contains(x)))
						throw new Exception("Получены ответы не на все вопросы");

					if (questions.Count() != questionIds.Count)
						throw new Exception("Среди ответов есть вопросы из другого опроса");

					foreach (var answer in data.Answers)
					{
						var userAnswer = new UserAnswer
						{
							AnswerId = answer.AnswerId,
							QuestionId = answer.QuestionId,
							SurveyId = data.SurveyId,
							TextAnswer = answer.Value,
							UserId = user.Id
						};

						await answerRepo.CreateAsync(userAnswer);
					}

					await _logService.CreateLog(new LogItemModel
					{ 
						MaterialId = data.EducationMaterialId,
						Type = (int)LogType.SurveyCompleted
					}, user.Id);

					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpPut]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> PutAsync([FromBody] SaveSurveyModel data)
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var surveyRepo = new BaseRepository<Survey>(db);
					var survey = await surveyRepo.GetByIdAsync(data.Id);

					if (survey == null)
						throw new Exception($"Опрос с id = {data.Id} не найден");

					if (survey.Title != data.Title)
					{
						survey.Title = data.Title;
						await surveyRepo.UpdateAsync(survey);
					}

					await RemoveAllQuestionsAndAnswers(data.Id, db);
					var questionRepo = new BaseRepository<Question>(db);
					var answerRepo = new BaseRepository<Answer>(db);

					foreach (var q in data.Questions)
					{
						var hasAnswers = q.Answers != null && q.Answers.Count > 0;

						var question = new Question
						{
							SurveyId = data.Id,
							Value = q.Title,
							HasAnswers = hasAnswers
						};

						var questionId = await questionRepo.CreateAsync(question);

						if (hasAnswers)
						{
							foreach (var a in q.Answers)
							{
								var answer = new Answer
								{
									QuestionId = questionId,
									Value = a
								};

								await answerRepo.CreateAsync(answer);
							}
						}
					}

					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpDelete]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteAsync(int id)
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					await RemoveAllQuestionsAndAnswers(id, db);
					var surveyRepo = new BaseRepository<Survey>(db);
					await surveyRepo.DeleteAsync(id);
					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		private async Task RemoveAllQuestionsAndAnswers(int surveyId, DataContext db)
		{
			var questionRepo = new BaseRepository<Question>(db);
			var questions = questionRepo.GetByPredicate(x => x.SurveyId == surveyId);

			var answerRepo = new BaseRepository<Answer>(db);
			var answers = answerRepo.GetByPredicate(x => questions.Select(x => x.Id).Contains(x.QuestionId));

			foreach (var answer in answers)
				await answerRepo.DeleteAsync(answer.Id);

			foreach (var question in questions)
				await questionRepo.DeleteAsync(question.Id);
		}
	}
}
