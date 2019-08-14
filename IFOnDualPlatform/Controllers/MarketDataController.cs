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
    public class MarketDataController : ControllerBase
    {
		private readonly ILogger<MarketDataController> _logger;
		private readonly ObtainStockQuote _obtainStockQuote;

		public MarketDataController(ILogger<MarketDataController> logger, ObtainStockQuote obtainStockQuote)
		{
			_logger = logger;
			_obtainStockQuote = obtainStockQuote;
		}
        // POST: api/MarketData
        [HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] MarketData marketData)
		{
			_logger.LogInformation("In Market Data Controller");
			string quotes = await _obtainStockQuote.GetMarketSummary();
			IAppResponse appResponse = new AppResponse
			{
				ResponseData = quotes,
				IsResponseSuccess = !quotes.IsNullOrWhiteSpace()
			};
			return Ok(appResponse);
		}
    }
}
