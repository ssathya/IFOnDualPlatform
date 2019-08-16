using Microsoft.Extensions.Logging;
using Models.Application;
using Models.MarketData;
using MongoHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities.Application;
using Utilities.StringHelpers;

namespace ExternalInterface.BusLogic
{
    public class ObtainGoodInvestments
    {
		private readonly ILogger<ObtainGoodInvestments> _logger;
		private readonly IDBConnectionHandler<PiotroskiScoreMd> _connectionHandlerCF;
		private readonly EnvHandler _envHandler;
		private readonly ResolveCompanyName _resolveCompanyName;
		private readonly ObtainStockQuote _obtainStockQuote;
		private readonly string iexCompanyStatssURL = @"https://cloud.iexapis.com/stable/stock/{ticker}/stats?token={api-key}";
		private const string apiKey = "{api-key}";
		private const string iexTradingProvider = "IEXTrading";


		public ObtainGoodInvestments(ILogger<ObtainGoodInvestments> logger,
			IDBConnectionHandler<PiotroskiScoreMd> connectionHandlerCF,
			ResolveCompanyName resolveCompanyName,
			ObtainStockQuote obtainStockQuote,
			EnvHandler envHandler)
		{
			_logger = logger;
			_connectionHandlerCF = connectionHandlerCF;
			_envHandler = envHandler;
			_resolveCompanyName = resolveCompanyName;
			_obtainStockQuote = obtainStockQuote;
			_ = _connectionHandlerCF.ConnectToDatabase("PiotroskiScore");
		}

		public async Task<string> SelectRandomGoodFirms()
		{
			_logger.LogInformation("Starting to select firms for the requester");
			List<PiotroskiScoreMd> selectedFirms;
			try
			{
				selectedFirms = _connectionHandlerCF.Get(r => r.Rating >= 7 && r.FYear == DateTime.Now.Year)
						.ToList();
				if (selectedFirms == null || !selectedFirms.Any())
				{
					selectedFirms = _connectionHandlerCF.Get(r => r.Rating >= 7 && r.FYear == DateTime.Now.Year - 1)
					.ToList();
				}
				if (!selectedFirms.Any())
				{
					return "";
				}
			}
			catch (Exception ex)
			{
				_logger.LogCritical("Error while getting data from MongoDb from Collection PiotroskiScore");
				_logger.LogCritical(ex.Message);
				return "";
			}
			selectedFirms.Shuffle();
			selectedFirms = selectedFirms.Take(4)
				.OrderByDescending(r => r.Rating)
				.ThenByDescending(r=>r.Revenue)
				.ToList();
			var messageString = new StringBuilder();
			messageString.Append("Here are a few recommendations for you.\n");
			foreach (var selectedFirm in selectedFirms)
			{
				var companyName = await _resolveCompanyName.ResolveTicker(selectedFirm.Ticker);
				var symbol = Regex.Replace(selectedFirm.Ticker, ".{1}", "$0 ");
				messageString.Append($"{companyName} with ticker {symbol} has Piotrosk-F Score of {selectedFirm.Rating}\n");
				QuotesFromWorldTrading quotes = await _obtainStockQuote.GetStockQuotes(selectedFirm.Ticker);
				var companyStats = await ObtainCompanyStats(selectedFirm.Ticker);
				if (quotes != null && quotes.Data.Any())
				{
					var currentPrice = (float)quotes.Data[0].Price;					
					messageString.Append($"It last traded at ${Math.Round(currentPrice,2)}. ");					
				}				
				if (companyStats != null && companyStats.week52high != 0)
				{
					var week52Low = Math.Round(companyStats.week52low, 2);
					var week52High = Math.Round(companyStats.week52high, 2);
					var peRatio = companyStats.peRatio == null ? 0 : companyStats.peRatio;
					messageString.Append($"52 week range is between ${week52Low} and {week52High} and P E Ratio is {peRatio}. ");
				}
				messageString.Append("\n\n");
			}
			return messageString.ToString();
		}
		private async Task<CompanyKeyStats> ObtainCompanyStats(string ticker)
		{
			var urlToUse = iexCompanyStatssURL.Replace("{ticker}", ticker)
				.Replace(apiKey, _envHandler.GetApiKey(iexTradingProvider));
			try
			{
				using (var hc = new HttpClient())
				{
					string data = "{}";
					data = await hc.GetStringAsync(urlToUse);
					var companyOverview = JsonConvert.DeserializeObject<CompanyKeyStats>(data);
					return companyOverview;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while getting data from IEX Trading");
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
				{
					_logger.LogError(ex.InnerException.Message);
				}
				return new CompanyKeyStats();
			}
		}

	}
}
