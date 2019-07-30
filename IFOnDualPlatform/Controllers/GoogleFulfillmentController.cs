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

namespace IFOnDualPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GoogleFulfillmentController : ControllerBase
	{
		private readonly ILogger<GoogleFulfillmentController> _logger;
		private readonly JsonParser jsonParser;
		#region Public Methods
		public GoogleFulfillmentController(ILogger<GoogleFulfillmentController> logger)
		{
			_logger = logger;
			jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
		}
		// POST: api/GoogleFulfillment
		[HttpPost]
		//public IActionResult Post([FromBody] WebhookRequest value)
		public IActionResult Post()
		{
			string requestBody = new StreamReader(Request.Body).ReadToEndAsync().Result;
			
			var parserResult = JObject.Parse(requestBody);
			var value = jsonParser.Parse<WebhookRequest>(requestBody);
			WebhookResponse response =  ProcessWebhookRequests(value);
			return Ok(response);
		}

		private WebhookResponse ProcessWebhookRequests(WebhookRequest value)
		{
			var intentName = value.QueryResult.Intent.DisplayName;
			IAppRequest iRequest = null;
			string controllerName = "";
			switch (intentName)
			{
				case "companyNews":
					controllerName = "companyNews";
					var company = value.QueryResult.Parameters.Fields["companyName"].ToString();
					iRequest = new CompanyNews { CompanyName = company };
					break;
				case "newsFetch":					
					break;
				default:
					break;
			}			
			controllerName = "/api/" + controllerName;
			var baseURL = Request.Host.ToString();
			var keyUsedToAccess = Request.Headers["key"].ToString();
			var clinet = new RestClient("https://" + baseURL);
			var request = new RestRequest(controllerName, Method.POST);
			request.AddHeader("key", keyUsedToAccess.IsNullOrWhiteSpace() ? "" : keyUsedToAccess);
			request.AddJsonBody(iRequest);
			var response = clinet.Execute<AppResponse>(request).Data;
			if (response.IsResponseSuccess)
			{
				var returnValue = new WebhookResponse
				{
					FulfillmentText = response.ResponseData
				};
				return returnValue;
			}
			throw new NotImplementedException();
		}

		#endregion Public Methods
	}
}