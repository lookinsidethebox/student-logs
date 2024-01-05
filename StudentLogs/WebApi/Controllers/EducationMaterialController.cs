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
		private readonly IWebHostEnvironment _webHostEnvironment;

		public EducationMaterialController(IDataContextOptionsHelper dataContextOptionsHelper,
			ILogger<EducationMaterialController> logger,
			IWebHostEnvironment webHostEnvironment)
		{
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_logger = logger;
			_webHostEnvironment = webHostEnvironment;
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

		[HttpGet]
		[Route("byId")]
		public async Task<IActionResult> GetByIdAsync(int id)
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<EducationMaterial>(db);
					var material = await repo.GetByIdAsync(id);
					return Ok(material);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}

		[HttpPost]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> PostAsync([FromForm] EducationMaterialModel data)
		{
			try
			{
				if (string.IsNullOrEmpty(data.Title) || data.Type == (int)EducationMaterialType.NotSet)
					throw new Exception("Не заданы обязательные поля Заголовок и Тип материала");

				if (data.Title.Length > 100)
					throw new Exception($"Заголовок не может быть длинее 100 символов. Сейчас {data.Title.Length} символов");

				if (!string.IsNullOrEmpty(data.Description) && data.Description.Length > 320)
					throw new Exception($"Описание не может быть длинее 320 символов. Сейчас {data.Description.Length} символов");

				if (string.IsNullOrEmpty(data.Text) && data.Type == (int)EducationMaterialType.Text)
					throw new Exception("Для материала с типом Текст необходимо заполнить поле Текст");

				if (!data.SurveyId.HasValue && data.Type == (int)EducationMaterialType.Survey)
					throw new Exception("Для материала с типом Опрос необходимо заполнить поле с выбором опроса");

				if (data.Type == (int)EducationMaterialType.Video && Request.Form.Files.Count == 0)
					throw new Exception("Для материала с типом Видео необходимо прикрепить файл с видео");

				if (data.Type == (int)EducationMaterialType.Document && Request.Form.Files.Count == 0)
					throw new Exception("Для материала с типом Документ необходимо прикрепить файл с документом");

				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<EducationMaterial>(db);
					var materialCount = (await repo.GetAsync()).Count() + 1;

					var material = new EducationMaterial
					{
						Title = data.Title,
						Description = data.Description,
						Type = (EducationMaterialType)data.Type,
						IsFirst = data.IsFirst,
						IsFinal = data.IsFinal,
						SurveyId = data.SurveyId,
						Text = data.Text,
						Order = (decimal)1 / materialCount,
						IsRequireOtherMaterials = data.IsRequireOtherMaterials,
						IsOneTime = data.IsOneTime
					};
					
					var materialId = await repo.CreateAsync(material);

					if (Request.Form.Files.Count > 0)
					{
						var file = Request.Form.Files.First();
						var fileModel = await UploadFile(file, materialId);

						if (fileModel != null)
						{
							material.FilePath = fileModel.Path;
							material.FileContentType = fileModel.ContentType;
							await repo.UpdateAsync(material);
						}
					}

					return Ok(new
					{
						Id = materialId,
						Title = material.Title
					});
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
					var materials = (await repo.GetAsync()).OrderByDescending(x => x.Order);
					var current = materials.Where(x => x.Id == model.CurrentMaterialId).FirstOrDefault();

					if (current == null)
						throw new Exception("Карточка не найдена");

					EducationMaterial? prev = null;
					EducationMaterial? next = null;

					if (model.PreviousMaterialId.HasValue)
						prev = materials.Where(x => x.Id == model.PreviousMaterialId.Value).FirstOrDefault();
					
					if (model.NextMaterialId.HasValue)
						next = materials.Where(x => x.Id == model.NextMaterialId.Value).FirstOrDefault();

					if (next == null)
					{
						var last = materials.Last();

						if (last.Order == 0)
						{
							last.Order = materials.ElementAt(materials.Count() - 2).Order / 2;
							await repo.UpdateAsync(last);
						}

						current.Order = last.Order / 2;
					}
					else if (prev == null)
					{
						var first = materials.First();
						var second = materials.ElementAt(1);
						first.Order = (1 - second.Order) / 2 + second.Order;
						await repo.UpdateAsync(first);
						current.Order = 1;
					}
					else
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
		[DisableRequestSizeLimit]
		public async Task<IActionResult> PutAsync([FromForm] EducationMaterialModel data)
		{
			try
			{
				if (string.IsNullOrEmpty(data.Title) || data.Type == (int)EducationMaterialType.NotSet)
					throw new Exception("Не заданы обязательные поля Заголовок и Тип материала");

				if (data.Title.Length > 100)
					throw new Exception($"Заголовок не может быть длинее 100 символов. Сейчас {data.Title.Length} символов");

				if (!string.IsNullOrEmpty(data.Description) && data.Description.Length > 320)
					throw new Exception($"Описание не может быть длинее 320 символов. Сейчас {data.Description.Length} символов");

				if (string.IsNullOrEmpty(data.Text) && data.Type == (int)EducationMaterialType.Text)
					throw new Exception("Для материала с типом Текст необходимо заполнить поле Текст");

				if (!data.SurveyId.HasValue && data.Type == (int)EducationMaterialType.Survey)
					throw new Exception("Для материала с типом Опрос необходимо заполнить поле с выбором опроса");
				
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
					material.IsOneTime = data.IsOneTime;
					material.IsRequireOtherMaterials = data.IsRequireOtherMaterials;

					if (Request.Form.Files.Count > 0)
					{
						var file = Request.Form.Files.First();
						var fileModel = await UploadFile(file, data.Id);

						if (fileModel != null)
						{
							material.FilePath = fileModel.Path;
							material.FileContentType = fileModel.ContentType;
						}
					}

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
					var logRepo = new BaseRepository<Log>(db);
					var logs = logRepo.GetByPredicate(x => x.EducationMaterialId == id);

					foreach (var log in logs)
						await logRepo.DeleteAsync(log.Id);

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

		private async Task<FileModel> UploadFile(IFormFile file, int materialId)
		{
			var filesFolderPath = $"{_webHostEnvironment.WebRootPath}/{FileHelper.FILES_PATH}";

			if (!Directory.Exists(filesFolderPath))
				Directory.CreateDirectory(filesFolderPath);

			var currentFileFolderPath = Path.Combine(filesFolderPath, materialId.ToString());

			if (!Directory.Exists(currentFileFolderPath))
				Directory.CreateDirectory(currentFileFolderPath);

			var filePath = Path.Combine(currentFileFolderPath, file.FileName);

			if (file != null && file.Length > 0)
			{
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);

					return new FileModel
					{ 
						ContentType = file.ContentType,
						Path = filePath
					};
				}
			}

			return null;
		}

		[HttpGet]
		[Route("reorder")]
		public async Task<IActionResult> ReorderAsync()
		{
			try
			{
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<EducationMaterial>(db);
					var materials = (await repo.GetAsync()).OrderByDescending(x => x.Id);
					var count = materials.Count();

					if (count < 2)
						return Ok();

					decimal step = (decimal)1 / count;
					decimal currentOrder = step;

					foreach (var material in materials)
					{ 
						material.Order = currentOrder;
						await repo.UpdateAsync(material);
						currentOrder += step;
					}

					return Ok();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest();
			}
		}
	}
}
