using ExternalInterface.BusLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Reqeusts;
using System.Threading.Tasks;
using Utilities.StringHelpers;

namespace IFOnDualPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FundamentalsController : ControllerBase
	{
		private readonly ILogger<FundamentalsController> _logger;
		private readonly ObtainFundamentals _obtainFundamentals;

		public FundamentalsController(ILogger<FundamentalsController> logger
			, ObtainFundamentals obtainFundamentals
			)
		{
			_logger = logger;
			_obtainFundamentals = obtainFundamentals;
		}

		// POST: api/Fundamentals
		[HttpPost]
		public  async Task<IActionResult> PostAsync([FromBody] CompanyData requestedFirm)
		{
			_logger.LogInformation("In Company Fundamentals Controller");
			string fundamentalsStr = await _obtainFundamentals.GetCompanyFundamentals(requestedFirm.CompanyName);
			//string fundamentalsStr = "Hello!";
			IAppResponse appResponse = new AppResponse
			{
				ResponseData = fundamentalsStr,
				IsResponseSuccess = !fundamentalsStr.IsNullOrWhiteSpace()
			};
			return Ok(appResponse);
		}
	}
}