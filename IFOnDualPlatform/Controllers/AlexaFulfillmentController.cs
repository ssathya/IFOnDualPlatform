using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IFOnDualPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AlexaFulfillmentController : ControllerBase
	{
		private readonly ILogger<AlexaFulfillmentController> _logger;
		private readonly Random rnd;
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

		private string[] greetings = {
			"Welcome. If you need help just say help for available commands. What can I do for you now?",
			"I can get you market data. If you need assistance just say help. What do you want to do now?",
			"Welcome. Do you want to get market data? If you do not know how to use this feature just say help! How can I assist?"
		};

		

		private string[] stopMsgs =
												{
			"Have a good day.",
			"I'll be around.",
			"Hope you enjoyed using this feature",
			"See you!"
		};

		public AlexaFulfillmentController(ILogger<AlexaFulfillmentController> logger)
		{
			_logger = logger;
			rnd = new Random();
		}
		// POST: api/AlexaFulfillment
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

	}
}
