using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExternalInterface.BusLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Reqeusts;
using Utilities.StringHelpers;

namespace IFOnDualPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
		private readonly ILogger<RecommendationsController> _logger;
		private readonly ObtainGoodInvestments _obtainGoodInvestments;

		public RecommendationsController(ILogger<RecommendationsController> logger, ObtainGoodInvestments obtainGoodInvestments)
		{
			_logger = logger;
			_obtainGoodInvestments = obtainGoodInvestments;
		}
 
        // POST: api/Recommendations
        [HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] Recommendations recommendations)
        {
			_logger.LogInformation("Starting Recommendations request");
			string randomRecommendations = await _obtainGoodInvestments.SelectRandomGoodFirms();
			IAppResponse appResponse = new AppResponse
			{
				ResponseData = randomRecommendations,
				IsResponseSuccess = !randomRecommendations.IsNullOrWhiteSpace()
			};
			return Ok(appResponse);
		}
    }
}
