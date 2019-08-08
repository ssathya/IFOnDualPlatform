using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using RestSharp;
using Utilities.StringHelpers;
using Models.Reqeusts;
using Microsoft.Extensions.Logging;
using System.IO;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using Utilities.Application;
using IFOnDualPlatform.Methods;

namespace IFOnDualPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GoogleFulfillmentController : ControllerBase
	{

		#region Private Fields

		private readonly ILogger<GoogleFulfillmentController> _logger;
		private readonly ICommonMethods _commonMethods;
		private readonly JsonParser jsonParser;

		#endregion Private Fields


		#region Public Constructors

		public GoogleFulfillmentController(ILogger<GoogleFulfillmentController> logger, ICommonMethods commonMethods)
		{
			_logger = logger;
			_commonMethods = commonMethods;
			jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
		}

		#endregion Public Constructors

		#region Public Methods

		// POST: api/GoogleFulfillment
		[HttpPost]
		//public IActionResult Post([FromBody] WebhookRequest value)
		public IActionResult Post()
		{
			_logger.LogDebug("Entering Google Fulfillment Post");
			string requestBody = new StreamReader(Request.Body).ReadToEndAsync().Result;						
			var value = jsonParser.Parse<WebhookRequest>(requestBody);
			WebhookResponse response =  ProcessWebhookRequests(value);
			response = CheckAndAddEndOfMessage(response);
			var returnString = response.ToString();
			return new ContentResult
			{
				Content = returnString,
				ContentType = "application/json",
				StatusCode = 200
			};
		}

		#endregion Public Methods


		#region Private Methods

		

		private WebhookResponse CheckAndAddEndOfMessage(WebhookResponse response)
		{
			if (response.FulfillmentMessages.Count == 0 && 
				!response.FulfillmentText.Contains(Utility.EndOfCurrentRequest()))
			{
				response.FulfillmentText = response.FulfillmentText + "\n" +
					Utility.EndOfCurrentRequest();
			}
			return response;
		}

		private WebhookResponse ProcessWebhookRequests(WebhookRequest value)
		{
			var intentName = value.QueryResult.Intent.DisplayName;
			IAppRequest iRequest = null;
			string controllerName = "";
			_commonMethods.ProcessIntends(value, intentName, ref iRequest, ref controllerName);
			_commonMethods.SetupAPICall(iRequest, controllerName, out RestClient clinet, out RestRequest request, Request);
			var response = clinet.Execute<AppResponse>(request).Data;
			if (response != null && response.IsResponseSuccess)
			{
				var returnValue = new WebhookResponse
				{
					FulfillmentText = response.ResponseData
				};
				return returnValue;
			}
			return new WebhookResponse
			{
				FulfillmentText = Utility.ErrorReturnMsg() + Utility.EndOfCurrentRequest()
			};
		}

		

		#endregion Private Methods
	}
}