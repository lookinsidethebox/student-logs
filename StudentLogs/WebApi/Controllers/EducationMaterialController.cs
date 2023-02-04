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
	[Route("material")]
	[Authorize(Roles = "Admin")]
	public class EducationMaterialController : ControllerBase
	{
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;
		private readonly ILogger<EducationMaterialController> _logger;

		public EducationMaterialController(IDataContextOptionsHelper dataContextOptionsHelper,
			ILogger<EducationMaterialController> logger)
		{
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> GetAsync()
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<EducationMaterial>(db);
					var materials = await repo.GetAsync();
					return Ok(materials.OrderByDescending(x => x.Order));
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}

		[HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] EducationMaterialModel data)
		{
			try
			{
				if (string.IsNullOrEmpty(data.Title) || data.Type == (int)EducationMaterialType.NotSet)
					throw new Exception("Не заданы обязательные поля Заголовок и Тип материала");

				if (string.IsNullOrEmpty(data.Text) && data.Type == (int)EducationMaterialType.Text)
					throw new Exception("Для материала с типом Текст необходимо заполнить поле Текст");

				if (!data.SurveyId.HasValue && data.Type == (int)EducationMaterialType.Survey)
					throw new Exception("Для материала с типом Опрос необходимо заполнить поле с выбором опроса");

				//if (data.Type == (int)EducationMaterialType.Video)
				//	throw new Exception("Для материала с типом Видео необходимо прикрепить файл с видео");

				//if (data.Type == (int)EducationMaterialType.Document)
				//	throw new Exception("Для материала с типом Документ необходимо прикрепить файл с документом");

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var material = new EducationMaterial
					{
						Title = data.Title,
						Description = data.Description,
						Type = (EducationMaterialType)data.Type,
						IsFirst = data.IsFirst,
						IsFinal = data.IsFinal,
						SurveyId = data.SurveyId,
						Text = data.Text,
						//FilePath = ""
					};

					var repo = new BaseRepository<EducationMaterial>(db);
					material.Id = await repo.CreateAsync(material);
					return Ok(material);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpPost]
		[Route("order")]
		public async Task<IActionResult> ChangeOrderAsync(EducationMaterialOrderModel model)
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<EducationMaterial>(db);

					var materials = repo.GetByPredicate(x => x.Id == model.CurrentMaterialId
						|| x.Id == model.PreviousMaterialId
						|| x.Id == model.NextMaterialId);

					var current = materials.Where(x => x.Id == model.CurrentMaterialId).FirstOrDefault();
					var prev = materials.Where(x => x.Id == model.PreviousMaterialId).FirstOrDefault();
					var next = materials.Where(x => x.Id == model.NextMaterialId).FirstOrDefault();

					if (current == null || prev == null || next == null)
						throw new Exception("Карточка не найдена");

					current.Order = (prev.Order - next.Order) / 2 + next.Order;
					await repo.UpdateAsync(current);
					return await GetAsync();
				}
			}
			catch
			{
				return BadRequest();
			}
		}

		[HttpPut]
		public async Task<IActionResult> PutAsync([FromBody] EducationMaterialModel data)
		{
			try
			{
				if (string.IsNullOrEmpty(data.Title))
					throw new Exception("Не задано обязательное поле Title");

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<EducationMaterial>(db);
					var material = await repo.GetByIdAsync(data.Id);

					if (material == null)
						throw new Exception($"Учебный материал с id = {data.Id} не найден");

					material.Title = data.Title;
					material.Description = data.Description;
					material.IsFirst = data.IsFirst;
					material.IsFinal = data.IsFinal;
					material.SurveyId = data.SurveyId;
					material.Text = data.Text;
					//material.FilePath = "";
					await repo.UpdateAsync(material);
					return Ok(material);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteAsync(int id)
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<EducationMaterial>(db);
					await repo.DeleteAsync(id);
					return Ok();
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}
	}
}
