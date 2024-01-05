using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("report")]
	[Authorize(Roles = "Admin")]
	public class ReportController : ControllerBase
	{
		private readonly ILogger<ReportController> _logger;
		private readonly IReportService _reportService;

		public ReportController(ILogger<ReportController> logger,
			IReportService reportService)
		{
			_logger = logger;
			_reportService = reportService;
		}

		private const string CSV_CONTENT_TYPE = "text/csv";

		[HttpGet]
		[Route("summary")]
		public async Task<IActionResult> GetSummaryReportAsync()
		{
			try
			{
				var report = await _reportService.GenerateSummaryReportAsync();
				var name = await _reportService.GetReportName();
				return File(report, CSV_CONTENT_TYPE, name);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}

		[HttpGet]
		[Route("user")]
		public async Task<IActionResult> GetUserReportAsync(int id)
		{
			try
			{
				var report = await _reportService.GenerateUserReportAsync(id);
				var name = await _reportService.GetReportName(id);
				return File(report, CSV_CONTENT_TYPE, name);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				return BadRequest(ex.Message);
			}
		}
	}
}
