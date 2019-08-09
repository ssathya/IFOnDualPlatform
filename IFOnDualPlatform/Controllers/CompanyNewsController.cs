using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Reqeusts;
using Microsoft.Extensions.Logging;
using Utilities.Application;
using ExternalInterface.BusLogic;

namespace IFOnDualPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
	public class CompanyNewsController : ControllerBase
    {
		private readonly ILogger<CompanyNewsController> _logger;
		private readonly ObtainNews _obtainNews;

		public CompanyNewsController(ILogger<CompanyNewsController> logger, ObtainNews obtainNews)
		{
			_logger = logger;
			_obtainNews = obtainNews;


		}
       
        // POST: api/ExternalNews
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CompanyData companyNews)
        {
			_logger.LogInformation("In Company News Controller");
			string newsToReport = await _obtainNews.GetCompanyNewsAsync(companyNews.CompanyName);
			IAppResponse appResponse = new AppResponse
			{
				ResponseData = newsToReport,
				IsResponseSuccess = true
			};
			return Ok(appResponse);

		}

	}
}
