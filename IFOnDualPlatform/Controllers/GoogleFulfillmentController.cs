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

namespace IFOnDualPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GoogleFulfillmentController : ControllerBase
	{

		#region Private Fields

		private readonly ILogger<GoogleFulfillmentController> _logger;
		private readonly JsonParser jsonParser;

		#endregion Private Fields


		#region Public Constructors

		public GoogleFulfillmentController(ILogger<GoogleFulfillmentController> logger)
		{
			_logger = logger;
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
			
			var parserResult = JObject.Parse(requestBody);
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

		private static void ProcessIntends(WebhookRequest value, string intentName, ref IAppRequest iRequest, ref string controllerName)
		{
			switch (intentName)
			{
				case "companyNews":
					controllerName = "companyNews";
					var company = value.QueryResult.Parameters.Fields["companyName"].ToString();
					company = company.StripSpecialChar();
					iRequest = new CompanyNews { CompanyName = company };
					break;
				case "newsFetch":
					controllerName = "NewsFetch";
					var newsSource = value.QueryResult.Parameters.Fields["newsSource"].ToString();
					newsSource = newsSource.StripSpecialChar();
					iRequest = new StandardNews { NewsSource = newsSource };
					break;
				default:
					break;
			}
		}

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
			ProcessIntends(value, intentName, ref iRequest, ref controllerName);
			SetupAPICall(iRequest, controllerName, out RestClient clinet, out RestRequest request);
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

		private void SetupAPICall(IAppRequest iRequest, string controllerName, out RestClient clinet, out RestRequest request)
		{
			controllerName = "/api/" + controllerName;
			var baseURL = Request.Host.ToString();
			var keyUsedToAccess = Request.Headers["key"].ToString();
			clinet = new RestClient("https://" + baseURL);
			request = new RestRequest(controllerName, Method.POST);
			request.AddHeader("key", keyUsedToAccess.IsNullOrWhiteSpace() ? "" : keyUsedToAccess);
			request.AddJsonBody(iRequest);
			return;
		}

		#endregion Private Methods
	}
}