using Microsoft.Extensions.Logging;
using Models.MarketData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities.Application;
using Utilities.StringHelpers;

namespace ExternalInterface.BusLogic
{
	public class ObtainStockQuote
	{

		#region Private Fields

		private readonly EnvHandler _envHandler;
		private readonly ILogger<ObtainStockQuote> _logger;
		private readonly ResolveCompanyName _resolveCompanyName;
		private readonly string quotesAPI = @"https://www.worldtradingdata.com/api/v1/stock?symbol={tickersToUse}&api_token={apiKey}";

		#endregion Private Fields

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ObtainStockQuote"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="resolveCompanyName">Name of the resolve company.</param>
		/// <param name="envHandler">The env handler.</param>
		public ObtainStockQuote(ILogger<ObtainStockQuote> logger, ResolveCompanyName resolveCompanyName, EnvHandler envHandler)
		{
			_logger = logger;
			_envHandler = envHandler;
			_resolveCompanyName = resolveCompanyName;
		}

		#endregion Public Constructors

		#region Public Methods

		/// <summary>
		/// Gets the market data.
		/// </summary>
		/// <param name="companyName">Name of the company.</param>
		/// <returns></returns>
		public async Task<string> GetMarketData(string companyName)
		{
			_logger.LogTrace("Starting to obtain quotes");
			var tickersToUse = await _resolveCompanyName.ResolveCompanyNameOrTicker(companyName);
			if (tickersToUse.IsNullOrWhiteSpace())
			{
				return "";
			}
			var quotes = await GetStockQuotes(tickersToUse);
			var returnValueMsg = BuildOutputMsg(quotes);
			return returnValueMsg;
		}

		public async Task<string> GetMarketSummary()
		{
			_logger.LogTrace("Starting to obtain market summary");
			var tickersToUse = "^DJI,^IXIC,^INX";
			var quotes = await GetStockQuotes(tickersToUse);
			var returnValueMsg = BuildIndexMsg(quotes);
			return returnValueMsg;
		}

		/// <summary>
		/// Gets the stock quotes.
		/// </summary>
		/// <param name="tickersToUse">The tickers to use.</param>
		/// <returns></returns>
		public async Task<QuotesFromWorldTrading> GetStockQuotes(string tickersToUse)
		{
			var apiKey = _envHandler.GetApiKey("WorldTradingDataKey");

			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogError("Did not find api key; calls will fail");
			}
			string urlStr = quotesAPI.Replace("{tickersToUse}", tickersToUse)
							.Replace("{apiKey}", apiKey);
			string data = "{}";
			try
			{
				using (var httpClient = new HttpClient())
				{
					data = await httpClient.GetStringAsync(urlStr);
				}
				var parsedData = JObject.Parse(data);
				data = data.Replace(":\"N/A\"", ":0.0");
				var indexData = JsonConvert.DeserializeObject<QuotesFromWorldTrading>(data);
				return indexData;
			}
			catch (Exception ex)
			{
				_logger.LogCritical($"Error processing data from World Trading.\n{ex.Message}");
				return new QuotesFromWorldTrading
				{
					Data = new Datum[0]
				};
			}
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Builds the index MSG.
		/// </summary>
		/// <param name="indexData">The index data.</param>
		/// <returns></returns>
		private string BuildIndexMsg(QuotesFromWorldTrading indexData)
		{
			StringBuilder tmpStr = new StringBuilder();
			if (indexData.Data.Length >= 1)
			{
				var dateToUse = indexData.Data[0].Last_trade_time == null ? DateTime.Parse("01-01-2000") :
					(DateTime)indexData.Data[0].Last_trade_time;
				tmpStr.Append("As of ");
				tmpStr.Append($"{dateToUse.ToString("MMMM dd, hh:mm tt")} EST ");
			}
			foreach (var idxData in indexData.Data)
			{
				float price = idxData.Price != null ? (float)idxData.Price : 0;
				float dayChange = idxData.Day_change == null ? 0 : (float)idxData.Day_change;
				tmpStr.Append($"{idxData.Name}  was at  {Math.Round(price, 0)}. ");
				tmpStr.Append(idxData.Day_change > 0 ? " Up by " : "Down by ");
				tmpStr.Append($"{Math.Abs(Math.Round(dayChange, 0))} points.\n\n ");
			}

			return tmpStr.ToString();
		}

		/// <summary>
		/// Builds the output MSG.
		/// </summary>
		/// <param name="quotes">The quotes.</param>
		/// <returns></returns>
		private string BuildOutputMsg(QuotesFromWorldTrading quotes)
		{
			if (quotes.Data.Length == 0)
			{
				return "Could not resolve requested firm or ticker";
			}
			var tmpStr = new StringBuilder();
			var datumToUse = quotes.Data.ToList().OrderByDescending(x => x.Last_trade_time);

			var dateToUse = (DateTime)datumToUse.First().Last_trade_time != null ?
				(DateTime)datumToUse.First().Last_trade_time : DateTime.Parse("01-01-2000");

			tmpStr.Append($"As of {dateToUse.ToString("MMMM dd, hh:mm tt")} EST ");
			foreach (var quote in quotes.Data)
			{
				var a = quote.Last_trade_time != null ?
					(DateTime)quote.Last_trade_time : DateTime.Parse("01-01-2000");
				if (DateTime.Now.AddDays(-4) > a)
				{
					//if the quote is older than 4 days the most probably it is not an actively traded security.
					continue;
				}
				float price = quote.Price != null ? (float)quote.Price : 0;
				quote.Day_change = quote.Day_change == null ? 0 : quote.Day_change;
				var symbol = Regex.Replace(quote.Symbol, ".{1}", "$0 ");
				tmpStr.Append($"{quote.Name} with ticker {symbol} was traded at {price.ToString("c2")}.");
				tmpStr.Append(quote.Day_change > 0 ? " Up by " : " Down by ");
				tmpStr.Append($"{(Math.Abs((float)quote.Day_change)).ToString("n2")} points.\n\n ");
			}
			return tmpStr.ToString();
		}

		#endregion Private Methods
	}
}