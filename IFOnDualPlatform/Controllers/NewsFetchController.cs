using ExternalInterface.BusLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Reqeusts;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.StringHelpers;

namespace IFOnDualPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NewsFetchController : ControllerBase
	{
		private readonly ILogger<NewsFetchController> _logger;

		private readonly ObtainNews _obtainNews;

		public NewsFetchController(ILogger<NewsFetchController> logger, ObtainNews obtainNews)
		{
			_logger = logger;
			_obtainNews = obtainNews;
		}		

		// POST: api/NewsFetch
		[HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] StandardNews standardNews)
		{
			_logger.LogInformation("In News Fetch Controller");
			string newsToReport = await _obtainNews.GetExternalNews(standardNews.NewsSource);
			IAppResponse appResponse = new AppResponse
			{
				ResponseData = newsToReport,
				IsResponseSuccess = !newsToReport.IsNullOrWhiteSpace(),				
			};
			return Ok(appResponse);
		}		
	}
}
