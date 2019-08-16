using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models.Reqeusts;
using RestSharp;
using System;
using System.Linq;
using Utilities.StringHelpers;

namespace IFOnDualPlatform.Methods
{
	public class CommonMethods : ICommonMethods
	{
		private const string helpMsgInSsml = @"<speak>
		<s>
		<emphasis level='strong'>Index Flux</emphasis> will get equity market details and news that affect it. This application will be ever evolving and stay tuned for new features that’ll be added periodically.
		</s>
		<s>
		<break strength='strong'/>
		<break strength='medium'/>Phrases that Index flux understands are
		<voice name='Matthew'>How is the market doing? Get me the quote for GOOG, What is the latest price for Citigroup</voice>, etc. The application can get the quotes for all listed firms
		<break strength='medium'/>
		<break strength='strong'/>
		<break strength='medium'/>Index Flux can also analyze a firm’s fundamentals for you. To get a firm’s fundamentals say
		<voice name='Matthew'>Please get me the basic information about General Electric or What are the fundamentals for Best Buy</voice>.
		<break strength='weak'/> Currently, Index Flux analyzes around 2400 firms but the list is growing.
		<break time='1s'/>
		</s>
		<s>
		<break strength='medium'/>Try your luck; ask me
		<voice name='Matthew'>Get me some recommendations</voice> or
		<voice name='Matthew'>Recommend me some stocks to research</voice>
		</s>
		<s>
		<break time='1s'/>
		<break strength='medium'/>Also if you are interested in the latest headlines from CNBC, New York Times, Wall Street Journal, or The Hindu, ask me
		<voice name='Matthew'>Get me the news from CNBC</voice> or
		<voice name='Matthew'>Please get me the news from New York Times</voice>
		</s>
		<s>
		<break time='1s'/>
		<break strength='medium'/>You can quit the application by saying
		<prosody pitch='high'>bye, see you, or thank you</prosody>
		</s>
		</speak>";
		private readonly string[] fallbackMsgs =
		{
			"I didn't get that. Can you say it again?",
			"I missed what you said. What was that?",
			"Sorry, could you say that again?",
			"Sorry, can you say that again?",
			"Can you say that again?",
			"Sorry, I didn't get that. Can you rephrase?",
			"Sorry, what was that?",
			"One more time?",
			"What was that?",
			"Say that one more time?",
			"I didn't get that. Can you repeat?",
			"I missed that, say that again?"
		};

		



		private string[] stopMsgs =
												{
			"Have a good day.",
			"I'll be around.",
			"Hope you enjoyed using this feature",
			"See you!"
		};

		private readonly ILogger<CommonMethods> _logger;
		private readonly Random rnd;

		public CommonMethods(ILogger<CommonMethods> logger)
		{
			_logger = logger;
			rnd = new Random();
		}

		public void ProcessIntends(WebhookRequest value, string intentName, ref IAppRequest iRequest, ref string controllerName)
		{
			string entityOrSlot;
			switch (intentName)
			{
				case "companyNews":
					var company = value.QueryResult.Parameters.Fields["companyName"].ToString();
					entityOrSlot = company;
					break;

				case "newsFetch":
					var newsSource = value.QueryResult.Parameters.Fields["newsSource"].ToString();
					entityOrSlot = newsSource;
					break;
				case "stockQuote":
				case "fundamentals":
					var companyName = value.QueryResult.Parameters.Fields["CompanyName"].ToString();
					entityOrSlot = companyName;
					break;
				case "marketSummary":
					entityOrSlot = "";
					break;
				case "recommend":
					entityOrSlot = "";
					break;
				default:
					return;
			}
			ProcessIntends(entityOrSlot, intentName, ref iRequest, ref controllerName);
		}

		public void ProcessIntends(SkillRequest skillRequest, ref SkillResponse response, string intentName, ref IAppRequest iRequest, ref string controllerName)
		{
			string entityOrSlot;
			var intentRequest = skillRequest.Request as IntentRequest;
			switch (intentName)
			{
				case "companyNews":
					var company = intentRequest.Intent.Slots["companyName"].Value;
					entityOrSlot = company;
					break;
				case "newsFetch":
					var newsSource = "google-news";
					var authories = intentRequest.Intent.Slots["newsSource"].Resolution.Authorities.FirstOrDefault();
					if (authories != null)
					{
						if (authories.Values.Any())
						{
							var source = authories.Values[0].Value.Name;
							newsSource = source;
						}
						
					}
					entityOrSlot = newsSource;
					break;
				case "stockQuote":
				case "fundamentals":
					company = intentRequest.Intent.Slots["CompanyName"].Value;
					entityOrSlot = company.StripSpecialChar();
					break;
				case "marketSummary":
					entityOrSlot = "";
					break;
				case "recommend":
					entityOrSlot = "";
					break;

				//Unique to Alexa
				case "AMAZON.FallbackIntent":
				case "FallbackIntent":
					var seeker = rnd.Next(fallbackMsgs.Length);
					response = ResponseBuilder.Tell(fallbackMsgs[seeker]);
					response.Response.ShouldEndSession = false;
					return;

				case "AMAZON.CancelIntent":
				case "CancelIntent":
					response = ResponseBuilder.Tell("Okay we'll terminate now!");
					response.Response.ShouldEndSession = true;
					return;

				case "AMAZON.HelpIntent":
				case "HelpIntent":
					var speech = new SsmlOutputSpeech
					{
						Ssml = helpMsgInSsml
					};
					response = ResponseBuilder.Tell(speech);
					response.Response.ShouldEndSession = false;
					return;

				case "AMAZON.StopIntent":
				case "StopIntent":
				case "AMAZON.NavigateHomeIntent":
				case "NavigateHomeIntent":
					seeker = rnd.Next(stopMsgs.Length);
					response = ResponseBuilder.Tell(stopMsgs[seeker]);
					response.Response.ShouldEndSession = true;
					_logger.LogDebug("Ending user session");
					return;
				default:
					return;
			}
			ProcessIntends(entityOrSlot, intentName, ref iRequest, ref controllerName);
		}

		public void SetupAPICall(IAppRequest iRequest, string controllerName, out RestClient clinet, out RestRequest request, HttpRequest webRequest)
		{
			controllerName = "/api/" + controllerName;
			var baseURL = webRequest.Host.ToString();
			var keyUsedToAccess = webRequest.Headers["key"].ToString();
			clinet = new RestClient("https://" + baseURL);
			request = new RestRequest(controllerName, Method.POST);
			request.AddHeader("key", keyUsedToAccess.IsNullOrWhiteSpace() ? "" : keyUsedToAccess);
			request.AddJsonBody(iRequest);
			return;
		}

		private void ProcessIntends(string entityOrSlot, string intentName, ref IAppRequest iRequest, ref string controllerName)
		{
			switch (intentName)
			{
				case "companyNews":
					controllerName = "companyNews";
					var company = entityOrSlot;
					company = company.StripSpecialChar();
					iRequest = new CompanyData { CompanyName = company };
					break;

				case "newsFetch":
					controllerName = "NewsFetch";
					var newsSource = entityOrSlot;
					newsSource = newsSource.StripSpecialChar();
					iRequest = new StandardNews { NewsSource = newsSource };
					break;
				case "stockQuote":
					controllerName = "StockQuote";
					company = entityOrSlot.StripSpecialChar();
					iRequest = new CompanyData { CompanyName = company };
					break;
				case "fundamentals":
					controllerName = "Fundamentals";
					company = entityOrSlot.StripSpecialChar();
					iRequest = new CompanyData { CompanyName = company };
					break;
				case "marketSummary":
					controllerName = "MarketData";
					iRequest = new MarketData();
					break;
				case "recommend":
					controllerName = "Recommendations";
					iRequest = new Recommendations();
					break;
				default:
					break;
			}
		}

	}
}