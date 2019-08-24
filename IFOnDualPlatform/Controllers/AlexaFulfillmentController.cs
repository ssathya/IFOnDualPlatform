using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using IFOnDualPlatform.Methods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Reqeusts;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Utilities.Application;
using Utilities.StringHelpers;

namespace IFOnDualPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AlexaFulfillmentController : ControllerBase
	{

		#region Private Fields

		private readonly ICommonMethods _commonMethods;
		private readonly ILogger<AlexaFulfillmentController> _logger;
		private readonly Random rnd;

		private string[] greetings = {
			"Welcome. If you need help just say help for available commands. What can I do for you now?",
			"I can get you market data. If you need assistance just say help. What do you want to do now?",
			"Welcome. Do you want to get market data? If you do not know how to use this feature just say help! How can I assist?"
		};

		#endregion Private Fields


		#region Public Constructors

		public AlexaFulfillmentController(ILogger<AlexaFulfillmentController> logger, ICommonMethods commonMethods)
		{
			_logger = logger;
			_commonMethods = commonMethods;
			rnd = new Random();
		}

		#endregion Public Constructors

		#region Public Methods

		// POST: api/AlexaFulfillment
		[HttpPost]
		public async Task<IActionResult> Post()
		{
			_logger.LogDebug("Entering Post request");			
			string json;
			using (var sr = new StreamReader(Request.Body))
			{
				json = await sr.ReadToEndAsync();
			}			
			var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);
			var isValid = await ValidateRequest(Request, _logger, skillRequest, json);
			if (!isValid)
			{
				return new BadRequestResult();
			}
			var requestType = skillRequest.GetRequestType();

			SkillResponse response;
			if (requestType == typeof(LaunchRequest))
			{
				response = LaunchRequestHandler();
			}
			else if (requestType == typeof(IntentRequest))
			{
				response = ParseIntents(skillRequest);
			}
			else
			{
				_logger.LogError("Error while parsing this request:");
				_logger.LogError(skillRequest.ToString());
				response = ErrorRequestHandler("Unknown Intent");
			}
			return new OkObjectResult(response);
		}

		#endregion Public Methods


		#region Private Methods

		private static async Task<bool> ValidateRequest(HttpRequest request, ILogger log, SkillRequest skillRequest, string body)
		{
			request.Headers.TryGetValue("SignatureCertChainUrl", out var signatureChainUrl);
			if (string.IsNullOrWhiteSpace(signatureChainUrl))
			{
				log.LogError("Validation failed. Empty SignatureCertChainUrl header");
				return false;
			}
			Uri certUrl;
			try
			{
				certUrl = new Uri(signatureChainUrl);
			}
			catch (Exception ex)
			{
				log.LogError($"Validation failed. SignatureChainUrl not valid: {signatureChainUrl}");
				log.LogError(ex.Message);
				return false;
			}
			request.Headers.TryGetValue("Signature", out var signature);
			if (string.IsNullOrWhiteSpace(signature))
			{
				log.LogError("Validation failed - Empty Signature header");
				return false;
			}
			if (body.IsNullOrWhiteSpace())
			{
				log.LogError("Validation failed - the JSON is empty");
				return false;
			}
			bool isTimestampValid = RequestVerification.RequestTimestampWithinTolerance(skillRequest);
			bool valid = await RequestVerification.Verify(signature, certUrl, body);
			if (!valid || !isTimestampValid)
			{
				log.LogError("Validation failed - RequestVerification failed");
				return false;
			}
			else
			{
				return true;
			}
		}

		private string CheckAndAddEndOfMessage(string response)
		{
			if (response.IsNullOrWhiteSpace())
			{
				return "\n\n" + Utility.ErrorReturnMsg();
			}
			else if (!response.Contains(Utility.EndOfCurrentRequest()))
			{
				return response + "\n\n" + Utility.EndOfCurrentRequest();
			}
			return response;
		}

		private SkillResponse ErrorRequestHandler(string intentName)
		{
			SkillResponse skillResponse;
			var returnMsg = new StringBuilder();
			returnMsg.Append("An internal error occurred.\n");
			returnMsg.Append($"Your request came with the intent name {intentName}.\n");
			returnMsg.Append($"We'll end this session now; please visit again for better interaction. Thank you and sorry for disappointing you.");
			skillResponse = ResponseBuilder.Tell(returnMsg.ToString());
			_logger.LogCritical(returnMsg.ToString());
			skillResponse.Response.ShouldEndSession = true;
			return skillResponse;
		}

		private SkillResponse LaunchRequestHandler()
		{
			var seeker = rnd.Next(greetings.Length);
			SkillResponse response = ResponseBuilder.Tell(greetings[seeker]);
			response.Response.ShouldEndSession = false;
			return response;
		}

		private SkillResponse ParseIntents(SkillRequest skillRequest)
		{
			SkillResponse skillResponse = null;
			var intentRequest = skillRequest.Request as IntentRequest;
			var intentName = intentRequest.Intent.Name;
			IAppRequest iRequest = null;
			string controllerName = "";
			_commonMethods.ProcessIntends(skillRequest, ref skillResponse, intentName, ref iRequest, ref controllerName);
			if (skillResponse != null)
			{
				skillResponse.SessionAttributes = skillRequest.Session.Attributes;
				return skillResponse;
			}
			_commonMethods.SetupAPICall(iRequest, controllerName, out RestClient clinet, out RestRequest request, Request);
			var response = clinet.Execute<AppResponse>(request).Data;
			if (response != null && response.IsResponseSuccess)
			{
				var returnMsg = response.ResponseData.ConvertAllToASCII();
				returnMsg = CheckAndAddEndOfMessage(returnMsg);
				returnMsg = returnMsg.ConvertToSSML();
				var speech = new SsmlOutputSpeech
				{
					Ssml = returnMsg
				};
				skillResponse = ResponseBuilder.Tell(speech, skillRequest.Session);
				skillResponse.Response.ShouldEndSession = response.ShouldEndSession;
			}
			else
			{
				_logger.LogError("Error while parsing  request:");
				_logger.LogError(skillRequest.ToString());
				skillResponse = ErrorRequestHandler(intentName);
				skillResponse.SessionAttributes = skillRequest.Session.Attributes;
			}
			return skillResponse;
		}

		#endregion Private Methods
	}
}