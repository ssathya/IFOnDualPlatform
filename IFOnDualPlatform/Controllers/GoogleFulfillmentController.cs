using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Mvc;

namespace IFOnDualPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GoogleFulfillmentController : ControllerBase
	{
		#region Public Methods

		// POST: api/GoogleFulfillment
		[HttpPost]
		public IActionResult Post([FromBody] WebhookRequest value)
		{
			var intentName = value.QueryResult.Intent.DisplayName;
			return Ok(intentName);
		}

		#endregion Public Methods
	}
}