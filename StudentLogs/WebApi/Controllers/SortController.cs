using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("sort")]
	public class SortController : ControllerBase
	{
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;

		public SortController(IDataContextOptionsHelper dataContextOptionsHelper)
		{
			_dataContextOptionsHelper = dataContextOptionsHelper;
		}

		[HttpGet]
		public IActionResult Get()
		{
			try
			{
				var email = HttpContext.User.Identity?.Name;
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<User>(db);
					var user = repo.GetByPredicate(x => x.Email == email).FirstOrDefault();

					if (user == null)
						throw new Exception();

					return Ok((int)user.SortType);
				}
			}
			catch
			{
				return BadRequest();
			}
		}

		[HttpPost]
		public async Task<IActionResult> PostAsync([FromForm] int type)
		{
			try
			{
				var email = HttpContext.User.Identity?.Name;
				var options = _dataContextOptionsHelper.GetDataContextOptions();

				using (var db = new DataContext(options))
				{
					var repo = new BaseRepository<User>(db);
					var user = repo.GetByPredicate(x => x.Email == email).FirstOrDefault();

					if (user == null)
						throw new Exception();

					user.SortType = (SortType)type;
					await repo.UpdateAsync(user);
					return Ok();
				}
			}
			catch
			{
				return BadRequest();
			}
		}
	}
}
