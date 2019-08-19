﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Utilities.Application;
using Utilities.StringHelpers;

namespace IFOnDualPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AlexaFulfillmentController : ControllerBase
	{
		private readonly ILogger<AlexaFulfillmentController> _logger;
		private readonly ICommonMethods _commonMethods;
		private readonly Random rnd;

		private string[] greetings = {
			"Welcome. If you need help just say help for available commands. What can I do for you now?",
			"I can get you market data. If you need assistance just say help. What do you want to do now?",
			"Welcome. Do you want to get market data? If you do not know how to use this feature just say help! How can I assist?"
		};
		public AlexaFulfillmentController(ILogger<AlexaFulfillmentController> logger, ICommonMethods commonMethods)
		{
			_logger = logger;
			_commonMethods = commonMethods;
			rnd = new Random();
		}
		// POST: api/AlexaFulfillment
		[HttpPost]
		public async Task<IActionResult> Post()
		{
			_logger.LogDebug("Entering Post request");
			//string json = new StreamReader(Request.Body).ReadToEndAsync().Result;
			string json;
			using (var sr = new StreamReader(Request.Body))
			{
				json = await sr.ReadToEndAsync();
			}

			var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);
			var requestType = skillRequest.GetRequestType();
			SkillResponse response;
			if (requestType == typeof(LaunchRequest))
			{
				response = LaunchRequestHandler();				
			}
			else if(requestType == typeof(IntentRequest))
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
				skillResponse = ResponseBuilder.Tell(speech);
				skillResponse.Response.ShouldEndSession = response.ShouldEndSession;
			}
			else
			{
				_logger.LogError("Error while parsing  request:");
				_logger.LogError(skillRequest.ToString());
				skillResponse = ErrorRequestHandler(intentName);
			}
			return skillResponse;
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
		private  SkillResponse LaunchRequestHandler()
		{
			var seeker = rnd.Next(greetings.Length);
			SkillResponse response = ResponseBuilder.Tell(greetings[seeker]);
			response.Response.ShouldEndSession = false;
			return response;
		}
	}
}
