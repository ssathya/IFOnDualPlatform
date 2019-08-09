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
    public class StockQuoteController : ControllerBase
    {
		private readonly ILogger<StockQuoteController> _logger;
		private readonly ObtainStockQuote _obtainStockQuote;

		public StockQuoteController(ILogger<StockQuoteController> logger, ObtainStockQuote obtainStockQuote)
		{
			_logger = logger;
			_obtainStockQuote = obtainStockQuote;
		}
        // POST: api/StockQuote
        [HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] CompanyData companyData)
		{
			_logger.LogInformation("In Company quote controller");
			string quotes = await _obtainStockQuote.GetMarketData(companyData.CompanyName);
			IAppResponse appResponse = new AppResponse
			{
				ResponseData = quotes,
				IsResponseSuccess = !quotes.IsNullOrWhiteSpace()
			};
			return Ok(appResponse);
		}

 
    }
}
